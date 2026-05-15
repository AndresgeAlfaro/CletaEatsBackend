namespace CletaEatsBackend.Infra;

/// <summary>
/// Credenciales Supabase solo en servidor. Preferir ServiceRoleKey para PostgREST (evita RLS).
/// </summary>
public class SupabaseConnectionOptions
{
    public string Url { get; set; } = "";
    /// <summary>JWT anon (pk del proyecto); sirve como respaldo si no hay service_role.</summary>
    public string AnonKey { get; set; } = "";
    /// <summary>JWT service_role desde Supabase → Settings → API (no exponer al cliente).</summary>
    public string ServiceRoleKey { get; set; } = "";

    public string RestApiKey =>
        string.IsNullOrWhiteSpace(ServiceRoleKey) ? AnonKey : ServiceRoleKey;
}
