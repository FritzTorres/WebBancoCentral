using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BancoCentralRD.Web.Models.Core
{
    public class Envios
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public string GetSaldo(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cuenta_id", out var sCuenta) || !Guid.TryParse(sCuenta, out var cuentaId))
                return "ERROR|code=FALTA_PARAMETRO|message=cuenta_id invalido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_obtener_saldo", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cuenta_id", cuentaId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read())
                            {
                                return $"OK|cuenta_id={cuentaId}|saldo=0.00|moneda=DOP|timestamp={UtcNowIso()}";
                            }

                            var saldoObj = rd["saldo"];
                            var saldo = saldoObj == DBNull.Value ? 0.00m : Convert.ToDecimal(saldoObj, CultureInfo.InvariantCulture);

                            return $"OK|cuenta_id={cuentaId}|saldo={saldo.ToString("0.00", CultureInfo.InvariantCulture)}|moneda=DOP|timestamp={UtcNowIso()}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (IsNoAutorizado(ex)) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetCliente(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cliente_id", out var sId) || !Guid.TryParse(sId, out var clienteId))
                return "ERROR|code=FALTA_PARAMETRO|message=cliente_id invalido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand(
                        @"SELECT id, cedula_rnc, nombre_completo, tipo_cliente, kyc_vigente_hasta
                      FROM clientes WHERE id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", clienteId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_ENCONTRADO|message=Cliente no existe";

                            var ced = SafeStr(rd["cedula_rnc"]);
                            var nom = SafeStr(rd["nombre_completo"]);
                            var tip = SafeStr(rd["tipo_cliente"]);
                            var kyc = rd["kyc_vigente_hasta"] == DBNull.Value ? "" :
                                      Convert.ToDateTime(rd["kyc_vigente_hasta"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                            return $"OK|cliente_id={clienteId}|cedula_rnc={ced}|nombre={Escape(nom)}|tipo={tip}|kyc_vigente_hasta={kyc}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetCuenta(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cuenta_id", out var sId) || !Guid.TryParse(sId, out var cuentaId))
                return "ERROR|code=FALTA_PARAMETRO|message=cuenta_id invalido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand(
                        @"SELECT a.id, a.cliente_id, p.codigo AS producto, a.moneda, a.estado, a.abierta_en
                      FROM cuentas a
                      JOIN productos p ON p.id = a.producto_id
                      WHERE a.id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", cuentaId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read()) return "ERROR|code=NO_ENCONTRADO|message=Cuenta no existe";

                            var cliente = rd["cliente_id"] == DBNull.Value ? "" : rd["cliente_id"].ToString();
                            var prod = SafeStr(rd["producto"]);
                            var mon = SafeStr(rd["moneda"]);
                            var est = SafeStr(rd["estado"]);
                            var open = Convert.ToDateTime(rd["abierta_en"]).ToUniversalTime().ToString("o");

                            return $"OK|cuenta_id={cuentaId}|cliente_id={cliente}|producto={prod}|moneda={mon}|estado={est}|abierta_en={open}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetMovimientos(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cuenta_id", out var sId) || !Guid.TryParse(sId, out var cuentaId))
                return "ERROR|code=FALTA_PARAMETRO|message=cuenta_id invalido";

            DateTime? desde = kv.TryGetValue("desde", out var sDesde) && DateTime.TryParse(sDesde, out var d) ? d : (DateTime?)null;
            DateTime? hasta = kv.TryGetValue("hasta", out var sHasta) && DateTime.TryParse(sHasta, out var h) ? h : (DateTime?)null;

            int page = (kv.TryGetValue("page", out var sPage) && int.TryParse(sPage, out var p) && p > 0) ? p : 1;
            int size = (kv.TryGetValue("page_size", out var sSz) && int.TryParse(sSz, out var z) && z > 0 && z <= 100) ? z : 50;

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_get_movimientos", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cuenta_id", cuentaId);
                        cmd.Parameters.AddWithValue("@desde", (object)desde ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@hasta", (object)hasta ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@page", page);
                        cmd.Parameters.AddWithValue("@page_size", size);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                        {
                            var rows = new List<Dictionary<string, object>>();

                            while (rd.Read())
                            {
                                rows.Add(new Dictionary<string, object>
                                {
                                    ["transaccion_id"] = rd["transaccion_id"],
                                    ["cuenta_id"] = rd["cuenta_id"],
                                    ["debito"] = rd["debito"],
                                    ["credito"] = rd["credito"],
                                    ["moneda"] = rd["moneda"],
                                    ["contabilizado_en"] = Convert.ToDateTime(rd["contabilizado_en"]).ToUniversalTime().ToString("o")
                                });
                            }

                            int total = 0;
                            if (rd.NextResult() && rd.Read())
                            {
                                total = Convert.ToInt32(rd["total"]);
                            }

                            var sb = new StringBuilder();
                            sb.Append($"OK|page={page}|page_size={size}|total={total}");
                            for (int i = 0; i < rows.Count; i++)
                            {
                                var r = rows[i];
                                int n = i + 1;
                                sb.Append($"|r{n}_tx={r["transaccion_id"]}");
                                sb.Append($"|r{n}_debito={((decimal)r["debito"]).ToString("0.00", CultureInfo.InvariantCulture)}");
                                sb.Append($"|r{n}_credito={((decimal)r["credito"]).ToString("0.00", CultureInfo.InvariantCulture)}");
                                sb.Append($"|r{n}_moneda={r["moneda"]}");
                                sb.Append($"|r{n}_fecha={r["contabilizado_en"]}");
                            }
                            return sb.ToString();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (IsNoAutorizado(ex)) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetSaldoCorte(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cuenta_id", out var sId) || !Guid.TryParse(sId, out var cuentaId))
                return "ERROR|code=FALTA_PARAMETRO|message=cuenta_id invalido";
            if (!kv.TryGetValue("fecha", out var sFecha) || !DateTime.TryParse(sFecha, out var fecha))
                return "ERROR|code=FALTA_PARAMETRO|message=fecha invalida";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_get_saldo_corte", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cuenta_id", cuentaId);
                        cmd.Parameters.AddWithValue("@fecha", fecha);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read())
                                return $"OK|cuenta_id={cuentaId}|saldo=0.00|moneda=DOP|corte={fecha:yyyy-MM-dd}";

                            var saldo = rd["saldo"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["saldo"], CultureInfo.InvariantCulture);
                            return $"OK|cuenta_id={cuentaId}|saldo={saldo:0.00}|moneda=DOP|corte={fecha:yyyy-MM-dd}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + ex.Message;
            }
        }

        public string GetResumenCuentas(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("cliente_id", out var sId) || !Guid.TryParse(sId, out var clienteId))
                return "ERROR|code=FALTA_PARAMETRO|message=cliente_id invalido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_get_resumen_cuentas", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@cliente_id", clienteId);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                        {
                            var sb = new StringBuilder("OK");
                            int i = 0;
                            while (rd.Read())
                            {
                                i++;
                                var cuenta = rd["cuenta_id"];
                                var prod = SafeStr(rd["producto"]);
                                var mon = SafeStr(rd["moneda"]);
                                var est = SafeStr(rd["estado"]);
                                var saldo = Convert.ToDecimal(rd["saldo"], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture);

                                sb.Append($"|r{i}_cuenta_id={cuenta}|r{i}_producto={prod}|r{i}_moneda={mon}|r{i}_estado={est}|r{i}_saldo={saldo}");
                            }
                            sb.Append($"|count={i}");
                            return sb.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetIndicadores(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("desde", out var sDesde) || !DateTime.TryParse(sDesde, out var desde))
                return "ERROR|code=FALTA_PARAMETRO|message=desde invalido";
            if (!kv.TryGetValue("hasta", out var sHasta) || !DateTime.TryParse(sHasta, out var hasta))
                return "ERROR|code=FALTA_PARAMETRO|message=hasta invalido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_get_indicadores", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@desde", desde);
                        cmd.Parameters.AddWithValue("@hasta", hasta);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read())
                                return "OK|total_transacciones=0|monto_total=0.00";

                            var total = rd["total_transacciones"] == DBNull.Value ? 0 : Convert.ToInt32(rd["total_transacciones"]);
                            var monto = rd["monto_total"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["monto_total"], CultureInfo.InvariantCulture);
                            var first = rd["primera_tx"] == DBNull.Value ? "" : Convert.ToDateTime(rd["primera_tx"]).ToUniversalTime().ToString("o");
                            var last = rd["ultima_tx"] == DBNull.Value ? "" : Convert.ToDateTime(rd["ultima_tx"]).ToUniversalTime().ToString("o");

                            return $"OK|total_transacciones={total}|monto_total={monto:0.00}|primera_tx={first}|ultima_tx={last}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (IsNoAutorizado(ex)) return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string GetParametro(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("clave", out var clave) || string.IsNullOrWhiteSpace(clave))
                return "ERROR|code=FALTA_PARAMETRO|message=clave requerido";

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_get_parametro", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        return rd.Read()
                            ? $"OK|clave={rd["clave"]}|valor={rd["valor"]}"
                            : "ERROR|code=NO_ENCONTRADO|message=parametro no existe";
                    }
                }
            }
        }

        public string GetEncaje(Guid sessionId, Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("institucion_id", out var sInst) || !Guid.TryParse(sInst, out var instId))
                return "ERROR|code=FALTA_PARAMETRO|message=institucion_id invalido";

            if (!kv.TryGetValue("fecha", out var sFecha) || !DateTime.TryParse(sFecha, out var fecha))
                return "ERROR|code=FALTA_PARAMETRO|message=fecha invalida";

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_get_encaje_cumplimiento", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@institucion_id", instId);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!rd.Read()) return "ERROR|code=NO_DATA|message=Sin datos";

                        var req = Convert.ToDecimal(rd["requerido"], CultureInfo.InvariantCulture);
                        var man = Convert.ToDecimal(rd["mantenido"], CultureInfo.InvariantCulture);
                        var dif = Convert.ToDecimal(rd["diferencia"], CultureInfo.InvariantCulture);
                        return $"OK|institucion_id={rd["institucion_id"]}|fecha={Convert.ToDateTime(rd["fecha"]).ToString("yyyy-MM-dd")}|requerido={req:0.00}|mantenido={man:0.00}|diferencia={dif:0.00}";
                    }
                }
            }
        }

        public string ListClientes(Guid sessionId, Dictionary<string, string> kv)
        {
            kv.TryGetValue("q", out var q);
            int page = (kv.TryGetValue("page", out var sp) && int.TryParse(sp, out var p) && p > 0) ? p : 1;
            int size = (kv.TryGetValue("page_size", out var ss) && int.TryParse(ss, out var s) && s > 0 && s <= 200) ? s : 50;

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_list_clientes", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@q", (object)q ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@page", page);
                    cmd.Parameters.AddWithValue("@page_size", size);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        var rows = new List<Dictionary<string, object>>();
                        while (rd.Read())
                        {
                            rows.Add(new Dictionary<string, object>
                            {
                                ["cliente_id"] = rd["id"],
                                ["cedula_rnc"] = rd["cedula_rnc"],
                                ["nombre_completo"] = rd["nombre_completo"],
                                ["tipo_cliente"] = rd["tipo_cliente"],
                                ["creado_en"] = rd["creado_en"]
                            });
                        }
                        int total = 0; if (rd.NextResult() && rd.Read()) total = Convert.ToInt32(rd["total"]);

                        var sb = new StringBuilder($"OK|page={page}|page_size={size}|total={total}");
                        for (int i = 0; i < rows.Count; i++)
                        {
                            var r = rows[i]; int n = i + 1;
                            sb.Append($"|r{n}_cliente_id={r["cliente_id"]}|r{n}_cedula_rnc={r["cedula_rnc"]}|r{n}_nombre_completo={r["nombre_completo"]}|r{n}_tipo_cliente={r["tipo_cliente"]}|r{n}_creado_en={Convert.ToDateTime(r["creado_en"]):yyyy-MM-dd}");
                        }
                        return sb.ToString();
                    }
                }
            }
        }

        public string ListCuentas(Guid sessionId, Dictionary<string, string> kv)
        {
            Guid? clienteId = null; if (kv.TryGetValue("cliente_id", out var sc) && Guid.TryParse(sc, out var g)) clienteId = g;
            kv.TryGetValue("moneda", out var mon);
            kv.TryGetValue("producto", out var prod);
            int page = (kv.TryGetValue("page", out var sp) && int.TryParse(sp, out var p) && p > 0) ? p : 1;
            int size = (kv.TryGetValue("page_size", out var ss) && int.TryParse(ss, out var s) && s > 0 && s <= 200) ? s : 50;

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_list_cuentas", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@cliente_id", (object)clienteId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@moneda", (object)mon ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@producto", (object)prod ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@page", page);
                    cmd.Parameters.AddWithValue("@page_size", size);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        var rows = new List<Dictionary<string, object>>();
                        while (rd.Read())
                        {
                            rows.Add(new Dictionary<string, object>
                            {
                                ["cuenta_id"] = rd["cuenta_id"],
                                ["cliente_id"] = rd["cliente_id"],
                                ["institucion_id"] = rd["institucion_id"],
                                ["producto"] = rd["producto"],
                                ["moneda"] = rd["moneda"],
                                ["estado"] = rd["estado"],
                                ["saldo"] = rd["saldo"]
                            });
                        }
                        int total = 0; if (rd.NextResult() && rd.Read()) total = Convert.ToInt32(rd["total"]);

                        var sb = new StringBuilder($"OK|page={page}|page_size={size}|total={total}");
                        for (int i = 0; i < rows.Count; i++)
                        {
                            var r = rows[i]; int n = i + 1;
                            sb.Append($"|r{n}_cuenta_id={r["cuenta_id"]}|r{n}_cliente_id={r["cliente_id"]}|r{n}_producto={r["producto"]}|r{n}_moneda={r["moneda"]}|r{n}_estado={r["estado"]}|r{n}_saldo={Convert.ToDecimal(r["saldo"]):0.00}");
                        }
                        return sb.ToString();
                    }
                }
            }
        }

        public string ListTransacciones(Guid sessionId, Dictionary<string, string> kv)
        {
            DateTime? desde = null, hasta = null;
            if (kv.TryGetValue("desde", out var sd) && DateTime.TryParse(sd, out var d)) desde = d;
            if (kv.TryGetValue("hasta", out var sh) && DateTime.TryParse(sh, out var h)) hasta = h;
            kv.TryGetValue("tipo", out var tipo);
            Guid? clienteId = null; if (kv.TryGetValue("cliente_id", out var sc) && Guid.TryParse(sc, out var g)) clienteId = g;
            int page = (kv.TryGetValue("page", out var sp) && int.TryParse(sp, out var p) && p > 0) ? p : 1;
            int size = (kv.TryGetValue("page_size", out var ss) && int.TryParse(ss, out var s) && s > 0 && s <= 200) ? s : 50;

            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_list_transacciones", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cmd.Parameters.AddWithValue("@desde", (object)desde ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@hasta", (object)hasta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tipo", (object)tipo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cliente_id", (object)clienteId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@page", page);
                    cmd.Parameters.AddWithValue("@page_size", size);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        var rows = new List<Dictionary<string, object>>();
                        while (rd.Read())
                        {
                            rows.Add(new Dictionary<string, object>
                            {
                                ["transaccion_id"] = rd["transaccion_id"],
                                ["ref_externa"] = rd["ref_externa"],
                                ["tipo"] = rd["tipo"],
                                ["estado"] = rd["estado"],
                                ["monto_total"] = rd["monto_total"],
                                ["moneda"] = rd["moneda"],
                                ["creada_en"] = Convert.ToDateTime(rd["creada_en"]).ToUniversalTime().ToString("o")
                            });
                        }
                        int total = 0; if (rd.NextResult() && rd.Read()) total = Convert.ToInt32(rd["total"]);

                        var sb = new StringBuilder($"OK|page={page}|page_size={size}|total={total}");
                        for (int i = 0; i < rows.Count; i++)
                        {
                            var r = rows[i]; int n = i + 1;
                            sb.Append($"|r{n}_tx={r["transaccion_id"]}|r{n}_ref={r["ref_externa"]}|r{n}_tipo={r["tipo"]}|r{n}_estado={r["estado"]}|r{n}_monto={Convert.ToDecimal(r["monto_total"]):0.00}|r{n}_moneda={r["moneda"]}|r{n}_fecha={r["creada_en"]}");
                        }
                        return sb.ToString();
                    }
                }
            }
        }

        public string GetReservas(Guid sessionId, Dictionary<string, string> kv)
        {
            using (var cn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand("sp_get_reservas_por_moneda", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        var i = 0; var sb = new StringBuilder("OK");
                        while (rd.Read())
                        {
                            i++;
                            sb.Append($"|r{i}_moneda={rd["moneda"]}|r{i}_nombre={rd["nombre"]}|r{i}_saldo={Convert.ToDecimal(rd["saldo"], CultureInfo.InvariantCulture):0.00}");
                        }
                        sb.Append($"|count={i}");
                        return sb.ToString();
                    }
                }
            }
        }

        public string GetInstitucion(Guid sessionId, Dictionary<string, string> kv)
        {
            Guid? instId = null;
            if (kv.TryGetValue("institucion_id", out var sInst) && Guid.TryParse(sInst, out var g))
                instId = g;

            kv.TryGetValue("codigo_sib", out var codigo);

            if (instId == null && string.IsNullOrWhiteSpace(codigo))
                return "ERROR|code=FALTA_PARAMETRO|message=institucion_id o codigo_sib requerido";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_get_institucion", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@institucion_id", (object)instId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@codigo_sib", (object)codigo ?? DBNull.Value);

                        cn.Open();
                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read())
                                return "ERROR|code=NO_ENCONTRADA|message=institucion no existe";

                            var id = rd["institucion_id"].ToString();
                            var sib = rd["codigo_sib"].ToString();
                            var nom = rd["nombre"].ToString();
                            var tipo = rd["tipo"].ToString();
                            var act = (bool)rd["activo"] ? "1" : "0";
                            var crea = Convert.ToDateTime(rd["creado_en"]).ToUniversalTime().ToString("o");

                            return $"OK|institucion_id={id}|codigo_sib={sib}|nombre={nom}|tipo={tipo}|activo={act}|creado_en={crea}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.IndexOf("NO_AUTORIZADO", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "ERROR|code=NO_AUTORIZADO|message=No autorizado";
                if (ex.Message.IndexOf("FALTA_PARAMETRO", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "ERROR|code=FALTA_PARAMETRO|message=institucion_id o codigo_sib requerido";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        private static bool IsNoAutorizado(SqlException ex)
            => ex.Message.IndexOf("NO_AUTORIZADO", StringComparison.OrdinalIgnoreCase) >= 0;

        private static string UtcNowIso() => DateTime.UtcNow.ToString("o");

        private static string Sanitize(string s) =>
            (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();

        private static string SafeStr(object x) =>
            x == null || x == DBNull.Value ? "" : x.ToString();

        private static string Escape(string s) =>
            (s ?? "").Replace("|", @"\|").Replace("=", @"\=");
    }
}