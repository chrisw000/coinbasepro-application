namespace CoinbasePro.Application
{
    public class AppSetting
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
