using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BancoCentralRD.Web.Models.Core
{
    public sealed class CommandRouter
    {
        private readonly Auth _auth;
        private readonly Services _svc;
        private readonly Envios _env;

        public CommandRouter(Auth auth = null, Services svc = null, Envios env = null)
        {
            _auth = auth ?? new Auth();
            _svc = svc ?? new Services();
            _env = env ?? new Envios();
        }

        public string Handle(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "ERROR|code=VACIO|message=Mensaje vacío";

            var kv = Parse(raw);
            var cmd = GetCmd(kv);

            try
            {
                switch (cmd)
                {
                    case "PING":
                        return "OK|pong=1";

                    case "LOGIN":
                        return _auth.Login(kv);

                    case "LOGOUT":
                        return RequireSession(kv, sid => _auth.Logout(sid));

                    case "GET_SALDO":
                        return RequirePerm(kv, "CONSULTAR_SALDO", sid => _env.GetSaldo(sid, kv));

                    case "CREATE_INSTITUCION":
                        return RequirePerm(kv, "CONFIG_PARAMETROS", sid => _svc.CrearInstitucion(sid, kv));

                    case "GET_INSTITUCION":
                        return RequirePerm(kv, "CONSULTAR_INSTITUCIONES", sid => _env.GetInstitucion(sid, kv));

                    case "LIST_CLIENTES":
                        return RequirePerm(kv, "CONSULTAR_SALDO", sid => _env.ListClientes(sid, kv));

                    case "LIST_CUENTAS":
                        return RequirePerm(kv, "CONSULTAR_SALDO", sid => _env.ListCuentas(sid, kv));

                    case "LIST_TRANSACCIONES":
                        return RequirePerm(kv, "CONSULTAR_MOVIMIENTOS", sid => _env.ListTransacciones(sid, kv));

                    case "GET_MOVIMIENTOS":
                        return RequirePerm(kv, "CONSULTAR_MOVIMIENTOS", sid => _env.GetMovimientos(sid, kv));

                    case "GET_SALDO_CORTE":
                        return RequirePerm(kv, "CONSULTAR_SALDO", sid => _env.GetSaldoCorte(sid, kv));

                    case "GET_RESUMEN_CUENTAS":
                        return RequirePerm(kv, "CONSULTAR_SALDO", sid => _env.GetResumenCuentas(sid, kv));

                    case "GET_INDICADORES":
                        return RequirePerm(kv, "CONSULTAR_INDICADORES", sid => _env.GetIndicadores(sid, kv));

                    case "SET_PARAM":
                        return RequirePerm(kv, "CONFIG_PARAMETROS", sid => _svc.SetParametro(sid, kv));

                    case "GET_PARAM":
                        return RequirePerm(kv, "CONFIG_PARAMETROS", sid => _env.GetParametro(sid, kv));

                    case "GET_ENCAJE":
                        return RequirePerm(kv, "CONSULTAR_ENCAJE", sid => _env.GetEncaje(sid, kv));

                    case "POST_RTGS":
                        return RequirePerm(kv, "OPERAR_RTGS", sid => _svc.PostRTGS(sid, kv));

                    case "SET_TC":
                        return RequirePerm(kv, "CONFIG_PARAMETROS", sid => _svc.RegistrarTC(sid, kv));

                    case "GET_RESERVAS":
                        return RequirePerm(kv, "CONSULTAR_RESERVAS", sid => _env.GetReservas(sid, kv));

                    case "CREATE_CUSTOMER":
                        return RequirePerm(kv, "CREAR_CLIENTE", sid => _svc.CrearCliente(sid, kv));

                    case "OPEN_ACCOUNT":
                        return RequirePerm(kv, "ABRIR_CUENTA", sid => _svc.AbrirCuenta(sid, kv));

                    case "POST_TRANSACTION":
                        return RequirePerm(kv, "REGISTRAR_TRANSACCION", sid => _svc.RegistrarTransaccion(sid, kv));

                    case "REVERSE_TRANSACTION":
                        return RequirePerm(kv, "REVERSAR_TRANSACCION", sid => _svc.ReversarTransaccion(sid, kv));

                    case "ISSUE_CERT_SALDO":
                        return RequirePerm(kv, "EMITIR_CERT_SALDO", sid => _svc.EmitirCertSaldo(sid, kv));

                    case "ISSUE_CERT_SOLV":
                        return RequirePerm(kv, "EMITIR_CERT_SOLVENCIA", sid => _svc.EmitirCertSolvencia(sid, kv));

                    default:
                        return "ERROR|code=COMANDO_DESCONOCIDO|message=Comando no reconocido";
                }
            }
            catch (KeyNotFoundException ex)
            {
                return "ERROR|code=FALTA_PARAMETRO|message=" + Sanitize(ex.Message);
            }
            catch (FormatException ex)
            {
                return "ERROR|code=FORMATO_INVALIDO|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        private string RequireSession(Dictionary<string, string> kv, Func<Guid, string> action)
        {
            var sid = ReadSessionId(kv, required: true);
            var check = _auth.IsSessionValid(sid);
            if (!check) return "ERROR|code=SESION_INVALIDA|message=Sesión expirada o inexistente";
            return action(sid);
        }

        private string RequirePerm(Dictionary<string, string> kv, string permiso, Func<Guid, string> action)
        {
            var sid = ReadSessionId(kv, required: true);
            if (!_auth.IsSessionValid(sid))
                return "ERROR|code=SESION_INVALIDA|message=Sesión expirada o inexistente";

            if (!_auth.HasPermission(sid, permiso))
                return "ERROR|code=NO_AUTORIZADO|message=No tiene permiso " + permiso;

            return action(sid);
        }

        private static Guid ReadSessionId(Dictionary<string, string> kv, bool required)
        {
            if (!kv.TryGetValue("session_id", out var raw) || string.IsNullOrWhiteSpace(raw))
            {
                if (required) throw new KeyNotFoundException("session_id requerido");
                return Guid.Empty;
            }
            return Guid.Parse(raw);
        }

        private static string GetCmd(Dictionary<string, string> kv)
        {
            if (kv.TryGetValue("CMD", out var c) && !string.IsNullOrWhiteSpace(c))
                return c.Trim().ToUpperInvariant();

            if (kv.TryGetValue("__FIRST", out var f) && !string.IsNullOrWhiteSpace(f))
                return f.Trim().ToUpperInvariant();

            return "";
        }

        private static Dictionary<string, string> Parse(string raw)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parts = raw.Split('|');

            bool firstSet = false;
            foreach (var part in parts)
            {
                var p = part?.Trim();
                if (string.IsNullOrEmpty(p)) continue;

                int i = p.IndexOf('=');
                if (i > 0)
                {
                    var k = p.Substring(0, i).Trim();
                    var v = p.Substring(i + 1).Trim();
                    dict[k] = v;
                }
                else if (!firstSet)
                {
                    dict["__FIRST"] = p;
                    firstSet = true;
                }
            }

            return dict;
        }

        private static string Sanitize(string s) => (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();
    }
}