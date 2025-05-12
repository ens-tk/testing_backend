using System.Text.Json.Serialization;

namespace testing_back.Models
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusTask
    {
        Active,
        Completed,
        Overdue,
        Late
    }

}
