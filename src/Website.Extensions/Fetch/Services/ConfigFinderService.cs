using Microsoft.Extensions.Configuration;

namespace Website.Extensions.Fetch.Services;
public class ConfigFinderService {
  private readonly IConfiguration _config;

  public ConfigFinderService( IConfiguration config ) {
    _config = config;
  }

  public T? FindConfigFieldValue<T>( string field ) {
    return _config.GetValue<T>( field );
  }
}
