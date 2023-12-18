namespace WebApiGateway.Core.Converters;

using System.ComponentModel;
using Enumeration;
using Models.Submission;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

public class AbstractSubmissionConverter : CustomCreationConverter<AbstractSubmission>
{
    private SubmissionType _submissionType;

    public override bool CanWrite => false;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var submissionTypeString = obj.GetValue(nameof(SubmissionType), StringComparison.OrdinalIgnoreCase).Value<string>();

        if (!Enum.TryParse<SubmissionType>(submissionTypeString, out var submissionType) || !Enum.IsDefined(submissionType))
        {
            throw new ArgumentException("Submission type is not valid");
        }

        _submissionType = submissionType;

        return base.ReadJson(obj.CreateReader(), objectType, existingValue, serializer);
    }

    public override AbstractSubmission Create(Type objectType)
    {
        return _submissionType switch
        {
            SubmissionType.Producer => new PomSubmission(),
            SubmissionType.Registration => new RegistrationSubmission(),
            _ => throw new InvalidEnumArgumentException("Submission type is not mapped.")
        };
    }
}