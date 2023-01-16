using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReflectionSample
{
  public static class NetworkMonitor
  {
    private static NetworkMonitorSettings _networkMonitorSettings = new NetworkMonitorSettings();
    private static Type _warningServiceType;
    private static MethodInfo _warningServiceMethod;
    private static List<object> _warningServiceParameterValues = new List<object>();

    private static object _warningService;

    public static void Warn()
    {
      if (_warningService == null)
      {
        _warningService = Activator.CreateInstance(_warningServiceType);
      }

      var parameters = new List<object>();
      parameters.AddRange(_warningServiceParameterValues);

      _warningServiceMethod.Invoke(_warningService, parameters.ToArray());
    }
    public static void BootstrapFromConfiguration()
    {
      var appSettingsConfig = new ConfigurationBuilder()
      .AddJsonFile("./Configuration/appsettings.json", false, true)
      .Build();

      appSettingsConfig.Bind("NetworkMonitorSettings", _networkMonitorSettings);

      _warningServiceType = Assembly.GetEntryAssembly()
        .GetType(_networkMonitorSettings.WarningService);

      if (_warningServiceType is null)
      {
        throw new Exception("Configuration invalid - warning service not found");
      }

      _warningServiceMethod = _warningServiceType
        .GetMethod(_networkMonitorSettings.MethodToExecute);
      if (_warningServiceMethod is null)
      {
        throw new Exception("Configuration invalid - warning service method not found");
      }

      foreach (var parameterInfo in _warningServiceMethod.GetParameters())
      {
        if (!_networkMonitorSettings.PropertyBag.TryGetValue(parameterInfo.Name, out object parameterValue))
        {
          throw new Exception($"Configuration invalid - parameter {parameterInfo.Name} not found");
        }

        try
        {
          var typedValue = Convert.ChangeType(parameterValue, parameterInfo.ParameterType);
          _warningServiceParameterValues.Add(typedValue);
        }
        catch (System.Exception)
        {
          throw new Exception($"Configuration invalid - parameter {parameterInfo.Name} cannot be converted to expected type {parameterInfo.ParameterType}");
        }
      }
    }
  }
}