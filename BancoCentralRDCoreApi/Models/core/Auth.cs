using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace BancoCentralRD.Web.Models.Core
{
    public class Auth
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public string Login(Dictionary<string, string> kv)
        {
            if (!kv.TryGetValue("usuario", out var usuario) || string.IsNullOrWhiteSpace(usuario))
                return "ERROR|code=FALTA_PARAMETRO|message=usuario requerido";
            if (!kv.TryGetValue("contrasena", out var contrasena) || string.IsNullOrWhiteSpace(contrasena))
                return "ERROR|code=FALTA_PARAMETRO|message=contrasena requerida";

            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_login", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@contrasena", contrasena);
                        cn.Open();

                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (!rd.Read())
                                return "ERROR|code=LOGIN_INVALIDO|message=Credenciales inválidas";

                            var sesionId = rd["sesion_id"] is Guid g ? g : Guid.Parse(rd["sesion_id"].ToString());
                            var expira = rd["expira_en"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(rd["expira_en"], CultureInfo.InvariantCulture);

                            var expStr = expira.HasValue ? expira.Value.ToUniversalTime().ToString("o") : "";

                            return $"OK|session_id={sesionId}|expira_en={expStr}";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.IndexOf("LOGIN_INVALIDO", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "ERROR|code=LOGIN_INVALIDO|message=Credenciales inválidas";
                return "ERROR|code=SQL|message=" + Sanitize(ex.Message);
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public string Logout(Guid sessionId)
        {
            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("DELETE FROM sesiones WHERE id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", sessionId);
                        cn.Open();
                        var n = cmd.ExecuteNonQuery();
                        return n > 0 ? "OK|logout=1" : "OK|logout=0";
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR|code=EXCEPCION|message=" + Sanitize(ex.Message);
            }
        }

        public bool IsSessionValid(Guid sessionId)
        {
            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("SELECT expira_en FROM sesiones WHERE id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", sessionId);
                        cn.Open();
                        var obj = cmd.ExecuteScalar();
                        if (obj == null || obj == DBNull.Value) return false;

                        var exp = Convert.ToDateTime(obj, CultureInfo.InvariantCulture);
                        return exp > DateTime.UtcNow;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool HasPermission(Guid sessionId, string permisoClave)
        {
            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("sp_autorizar", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@sesion_id", sessionId);
                        cmd.Parameters.AddWithValue("@permiso_clave", permisoClave);
                        cn.Open();

                        using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (rd.Read())
                            {
                                var val = Convert.ToInt32(rd["autorizado"]);
                                return val == 1;
                            }
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public Guid GetUserId(Guid sessionId)
        {
            try
            {
                using (var cn = new SqlConnection(ConnStr))
                {
                    using (var cmd = new SqlCommand("SELECT usuario_id FROM sesiones WHERE id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", sessionId);
                        cn.Open();
                        var obj = cmd.ExecuteScalar();
                        return obj is Guid g ? g : Guid.Empty;
                    }
                }
            }
            catch
            {
                return Guid.Empty;
            }
        }

        private static string Sanitize(string s) =>
            (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();
    }
}