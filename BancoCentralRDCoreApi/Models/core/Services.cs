using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace BancoCentralRD.Web.Models.Core
{
    public class Services
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public string CrearCliente(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cedula_rnc", out var cedula) || string.IsNullOrWhiteSpace(cedula))
                return "ERROR|code=FALTA_PARAMETRO|message=cedula_rnc requerido";
            if (!kv.TryGetValue("nombre_completo", out var nombre) || string.IsNullOrWhiteSpace(nombre))
                return "ERROR|code=FALTA_PARAMETRO|message=nombre_completo requerido";
            if (!kv.TryGetValue("tipo_cliente", out var tipo) || string.IsNullOrWhiteSpace(tipo))
                return "ERROR|code=FALTA_PARAMETRO|message=tipo_cliente requerido";

            DateTime? kyc = TryParseDate(kv, "kyc_vigente_hasta");

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_crear_cliente", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cedula_rnc", cedula);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@tipo", tipo);
                        cmd.Parameters.AddWithValue("@kyc_vigente_hasta", (object)kyc ?? DBNull.Value);

                        cn.Open();
                        var rd = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        if (!rd.Read()) return "ERROR|code=NO_INSERTADO|message=No se pudo crear el cliente";

                        var id = rd["cliente_id"].ToString();
                        return $"OK|cliente_id={id}|cedula_rnc={cedula}";

                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "CLIENTE_EXISTE")) return "ERROR|code=CLIENTE_EXISTE|message=La cédula/RNC ya existe";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string AbrirCuenta(Guid sessionId, Dictionary<string, string> kv)
        {
            Guid? clienteId = TryParseGuid(kv, "cliente_id");
            if (!kv.TryGetValue("producto", out var prod) || string.IsNullOrWhiteSpace(prod))
                return "ERROR|code=FALTA_PARAMETRO|message=producto requerido";
            var moneda = kv.ContainsKey("moneda") && !string.IsNullOrWhiteSpace(kv["moneda"]) ? kv["moneda"] : "DOP";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_abrir_cuenta", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cliente_id", (object)clienteId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@producto", prod);
                        cmd.Parameters.AddWithValue("@moneda", moneda);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_INSERTADO|message=No se pudo abrir la cuenta";

                            var id = rd["cuenta_id"].ToString();
                            return $"OK|cuenta_id={id}|producto={prod}|moneda={moneda}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "PRODUCTO_NO_EXISTE")) return "ERROR|code=PRODUCTO_NO_EXISTE|message=Producto no existe";
                if (Has(ex, "KYC_NO_VIGENTE")) return "ERROR|code=KYC_NO_VIGENTE|message=KYC no vigente";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string RegistrarTransaccion(Guid sessionId, Dictionary<string, string> kv)
        {
            var refExt = kv.ContainsKey("ref_externa") ? kv["ref_externa"] : null;
            if (!kv.TryGetValue("tipo", out var tipo) || string.IsNullOrWhiteSpace(tipo))
                return "ERROR|code=FALTA_PARAMETRO|message=tipo requerido";
            var moneda = kv.ContainsKey("moneda") && !string.IsNullOrWhiteSpace(kv["moneda"]) ? kv["moneda"] : "DOP";

            if (!kv.TryGetValue("n", out var sN) || !int.TryParse(sN, out var n) || n <= 0)
                return "ERROR|code=FALTA_PARAMETRO|message=n (cantidad de líneas) inválido";

            var tvp = new DataTable();
            tvp.Columns.Add("cuenta_id", typeof(Guid));
            tvp.Columns.Add("debito", typeof(decimal));
            tvp.Columns.Add("credito", typeof(decimal));

            try
            {
                for (int i = 1; i <= n; i++)
                {
                    var acc = Guid.Parse(Expect(kv, $"line{i}_cuenta"));
                    var deb = ParseDecimal(Expect(kv, $"line{i}_debito"));
                    var cre = ParseDecimal(Expect(kv, $"line{i}_credito"));
                    tvp.Rows.Add(acc, deb, cre);
                }

                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_registrar_transaccion", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@ref_externa", (object)refExt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@tipo", tipo);
                        cmd.Parameters.AddWithValue("@moneda", moneda);
                        cmd.Parameters.AddWithValue("@glosa", kv.ContainsKey("glosa") ? (object)kv["glosa"] : DBNull.Value);

                        var p = cmd.Parameters.AddWithValue("@lineas", tvp);
                        p.SqlDbType = SqlDbType.Structured;
                        p.TypeName = "tvp_linea_asiento";

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_CONTABILIZADA|message=No se pudo contabilizar";

                            var id = rd["transaccion_id"].ToString();
                            return $"OK|transaccion_id={id}|monto_total={Convert.ToDecimal(rd["monto_total"], CultureInfo.InvariantCulture):0.00}|moneda={moneda}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "TRANSACCION_NO_CUADRA")) return "ERROR|code=TRANSACCION_NO_CUADRA|message=La transacción no cuadra (débitos ≠ créditos)";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string ReversarTransaccion(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("transaccion_id", out var sId) || !Guid.TryParse(sId, out var txnId))
                return "ERROR|code=FALTA_PARAMETRO|message=transaccion_id inválido";
            var motivo = kv.ContainsKey("motivo") ? kv["motivo"] : null;

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_reversar_transaccion", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@tx_id", txnId);
                        cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_REVERSADA|message=No se pudo reversar";

                            var id = rd["transaccion_id"].ToString();
                            return $"OK|reversa_id={id}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "TRANSACCION_NO_CONTABILIZADA")) return "ERROR|code=TRANSACCION_NO_CONTABILIZADA|message=La transacción no está contabilizada";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string EmitirCertSaldo(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cuenta_id", out var sId) || !Guid.TryParse(sId, out var cuentaId))
                return "ERROR|code=FALTA_PARAMETRO|message=cuenta_id inválido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_emitir_cert_saldo", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cuenta_id", cuentaId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_CERT|message=No se pudo emitir el certificado";

                            var id = rd["cert_id"].ToString();
                            return $"OK|certificado_id={id}|tipo=saldo";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "CUENTA_NO_ACTIVA")) return "ERROR|code=CUENTA_NO_ACTIVA|message=La cuenta no está activa";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string EmitirCertSolvencia(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cliente_id", out var sId) || !Guid.TryParse(sId, out var clienteId))
                return "ERROR|code=FALTA_PARAMETRO|message=cliente_id inválido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_emitir_cert_solvencia", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cliente_id", clienteId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_CERT|message=No se pudo emitir el certificado";

                            var id = rd["cert_id"].ToString();
                            return $"OK|certificado_id={id}|tipo=solvencia";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (Has(ex, "NO_AUTORIZADO")) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (Has(ex, "KYC_NO_VIGENTE")) return "ERROR|code=KYC_NO_VIGENTE|message=KYC no vigente";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex) { return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message); }
        }

        public string SetParametro(Guid sessionId, Dictionary<string, string> kv)
        {
            kv.TryGetValue("clave", out var clave);
            kv.TryGetValue("valor", out var valor);
            if (string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(valor))
                return "ERROR|code=FALTA_PARAMETRO|message=clave/valor requeridos";

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_set_parametro", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cmd.Parameters.AddWithValue("@valor", valor);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        return rd.Read() ? $"OK|clave={rd["clave"]}|valor={rd["valor"]}" : "OK|actualizado=1";
                    }
                }
            }
        }

        public string PostRTGS(Guid sessionId, Dictionary<string, string> kv)
        {
            kv.TryGetValue("ref_externa", out var refext);

            if (!kv.TryGetValue("origen", out var sOri) || !Guid.TryParse(sOri, out var ctaOri))
                return "ERROR|code=FALTA_PARAMETRO|message=origen invalido";

            if (!kv.TryGetValue("destino", out var sDes) || !Guid.TryParse(sDes, out var ctaDes))
                return "ERROR|code=FALTA_PARAMETRO|message=destino invalido";

            if (!kv.TryGetValue("monto", out var sMonto) || !decimal.TryParse(sMonto, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
                return "ERROR|code=FALTA_PARAMETRO|message=monto invalido";

            string moneda = "DOP";
            if (kv.TryGetValue("moneda", out var sMon) && !string.IsNullOrWhiteSpace(sMon)) moneda = sMon;

            kv.TryGetValue("glosa", out var glosa);

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_post_rtgs", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@origen", ctaOri);
                    cmd.Parameters.AddWithValue("@destino", ctaDes);
                    cmd.Parameters.AddWithValue("@monto", monto);
                    cmd.Parameters.AddWithValue("@referencia", refext);
                    cmd.Parameters.AddWithValue("@moneda", moneda);
                    cmd.Parameters.AddWithValue("@glosa", (object)glosa ?? DBNull.Value);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        return rd.Read()
                            ? $"OK|transaccion_id={rd["transaccion_id"]}|monto={monto:0.00}|moneda={moneda}"
                            : "ERROR|code=RTGS|message=No liquidado";
                    }
                }
            }
        }

        public string RegistrarTC(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("moneda", out var moneda) || string.IsNullOrWhiteSpace(moneda))
                return "ERROR|code=FALTA_PARAMETRO|message=moneda requerida";

            if (!kv.TryGetValue("fecha", out var sFecha) || !DateTime.TryParse(sFecha, out var fecha))
                return "ERROR|code=FALTA_PARAMETRO|message=fecha invalida";

            if (!kv.TryGetValue("tc_compra", out var sCompra) || !decimal.TryParse(sCompra, NumberStyles.Any, CultureInfo.InvariantCulture, out var compra))
                return "ERROR|code=FALTA_PARAMETRO|message=tc_compra invalido";

            if (!kv.TryGetValue("tc_venta", out var sVenta) || !decimal.TryParse(sVenta, NumberStyles.Any, CultureInfo.InvariantCulture, out var venta))
                return "ERROR|code=FALTA_PARAMETRO|message=tc_venta invalido";

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_registrar_tc", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@moneda", moneda);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@tc_compra", compra);
                    cmd.Parameters.AddWithValue("@tc_venta", venta);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        return rd.Read()
                            ? $"OK|moneda={rd["moneda"]}|fecha={Convert.ToDateTime(rd["fecha"]).ToString("yyyy-MM-dd")}|tc_compra={compra}|tc_venta={venta}"
                            : "OK|actualizado=1";
                    }
                }
            }
        }

        public string CrearInstitucion(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("codigo_sib", out var codigo) || string.IsNullOrWhiteSpace(codigo))
                return "ERROR|code=FALTA_PARAMETRO|message=codigo_sib requerido";
            if (!kv.TryGetValue("nombre", out var nombre) || string.IsNullOrWhiteSpace(nombre))
                return "ERROR|code=FALTA_PARAMETRO|message=nombre requerido";
            if (!kv.TryGetValue("tipo", out var tipo) || string.IsNullOrWhiteSpace(tipo))
                return "ERROR|code=FALTA_PARAMETRO|message=tipo requerido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_crear_institucion", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@codigo_sib", codigo);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@tipo", tipo);
                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_INSERTADO|message=No se pudo crear institucion";
                            return $"OK|institucion_id={rd["institucion_id"]}|codigo_sib={rd["codigo_sib"]}|nombre={rd["nombre"]}|tipo={rd["tipo"]}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("INSTITUCION_EXISTE"))
                    return "ERROR|code=INSTITUCION_EXISTE|message=El codigo ya existe";
                if (ex.Message.Contains("NO_AUTORIZADO"))
                    return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                return "ERROR|code=SQL|message=" + ex.Message;
            }
        }

        private static Guid? TryParseGuid(Dictionary<string, string> kv, string key)
            => kv.TryGetValue(key, out var s) && Guid.TryParse(s, out var g) ? g : (Guid?)null;

        private static DateTime? TryParseDate(Dictionary<string, string> kv, string key)
            => kv.TryGetValue(key, out var s) && DateTime.TryParse(s, out var d) ? d : (DateTime?)null;

        private static string Expect(Dictionary<string, string> kv, string key)
        {
            if (!kv.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                throw new KeyNotFoundException($"{key} requerido");
            return v;
        }

        private static decimal ParseDecimal(string s)
        {
            if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                throw new FormatException($"decimal inválido: {s}");
            return d;
        }

        private static bool Has(SqlException ex, string token)
            => ex.Message.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string Sanitize(string s)
            => (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();

        // C#
        private static bool ReaderHasColumn(IDataRecord r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}