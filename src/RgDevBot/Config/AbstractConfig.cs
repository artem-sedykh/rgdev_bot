using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace RgDevBot.Config
{
    public abstract class AbstractConfig<T>
    {
        protected readonly ILogger _logger;
        public T ConfigValues { get; set; }

        private readonly object _lock = new object();
        private readonly string _fileName;

        protected AbstractConfig(ILogger logger, string fileName)
        {
            _logger = logger;
            _fileName = fileName ?? "dbproducts.json";
            ConfigValues = Load() ?? Activator.CreateInstance<T>();
        }

        protected T Load()
        {
            try
            {
                var js = File.ReadAllText(_fileName);
                return JsonConvert.DeserializeObject<T>(js);
            }
            catch (Exception e1)
            {
                _logger.Error(e1, "Ошибка загрузки файла.");
                Save();
                return default;
            }
        }

        public virtual void Save()
        {
            try
            {
                lock (_lock)
                {

                    File.WriteAllText(_fileName, 
                        JsonConvert.SerializeObject(ConfigValues, 
                            Formatting.Indented,
                            new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }
                        )
                    );
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка сохранения файла.");
            }
        }
    }
}
