namespace Arcane.Ingestion.Services.Base
{
    public interface IIngestionService<T>
    {
        public void Ingest(string destinationName, T row);
    }
}
