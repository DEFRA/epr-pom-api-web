using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Core.Converters;

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
            SubmissionType.Subsidiary => new SubsidiarySubmission(),
            _ => throw new InvalidEnumArgumentException("Submission type is not mapped.")
        };
    }
}