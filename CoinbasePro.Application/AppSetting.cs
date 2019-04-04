namespace CoinbasePro.Application
{
    public interface IAppSettingCandleMonitor
    {
        bool HasOverlayFastUpdate { get;set; }
    }

    public class AppSetting : IAppSettingCandleMonitor
    {
        private const string DefaultCsvPath = "/csv-data";

        private string _csvPath = DefaultCsvPath;

        public string CsvPath
        {
            get => _csvPath;
            set => _csvPath = string.IsNullOrEmpty(_csvPath) ? DefaultCsvPath : value;
        }

        public string MlModelsPath { get; set; }

        public bool HasOverlayFastUpdate { get; set; } = false;
    }
}
