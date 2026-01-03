using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RoboCopy_X.Helpers
{
    /// <summary>
    /// Helper class para gerenciar privilégios de administrador da aplicação
    /// </summary>
    public static class AdminPrivilegeHelper
    {
        /// <summary>
        /// Verifica se a aplicação está rodando com privilégios de administrador
        /// </summary>
        /// <returns>True se estiver rodando como admin, False caso contrário</returns>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reinicia a aplicação com privilégios de administrador
        /// </summary>
        /// <returns>True se conseguiu iniciar o processo, False caso contrário</returns>
        public static bool RestartAsAdmin()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName,
                    UseShellExecute = true,
                    Verb = "runas" // Solicita elevação de privilégios
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao reiniciar como admin: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tenta obter informações sobre o usuário atual
        /// </summary>
        /// <returns>Nome do usuário e se é admin</returns>
        public static (string Username, bool IsAdmin) GetCurrentUserInfo()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                var username = identity.Name?.Split('\\').Length > 1 
                    ? identity.Name.Split('\\')[1] 
                    : identity.Name ?? "Unknown";
                
                return (username, isAdmin);
            }
            catch
            {
                return ("Unknown", false);
            }
        }
    }
}
