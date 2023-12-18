namespace WebApiGateway.UnitTests.Core.Converters;

using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WebApiGateway.Core.Models.Submission;

[TestClass]
public class AbstractSubmissionConverterTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [TestMethod]
    public void ConverterRead_ThrowsArgumentException_WhenSubmissionTypeIsNotValidEnumValue()
    {
        // Arrange
        const string SerializedValue = "{ \"submissionType\": 9 }";

        // Act
        Action action = () => JsonConvert.DeserializeObject<AbstractSubmission>(SerializedValue);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("Submission type is not valid");
    }

    [TestMethod]
    public void ConverterRead_ReturnsPomSubmission_WhenSubmissionTypeIsProducer()
    {
        // Arrange
        var submission = _fixture.Create<PomSubmission>();
        var serializedValue = JsonConvert.SerializeObject(submission);

        // Act
        var result = JsonConvert.DeserializeObject<AbstractSubmission>(serializedValue);

        // Assert
        result.Should().BeOfType<PomSubmission>();
    }

    [TestMethod]
    public void ConverterRead_ReturnsRegistrationSubmission_WhenSubmissionTypeIsRegistration()
    {
        // Arrange
        var submission = _fixture.Create<RegistrationSubmission>();
        var serializedValue = JsonConvert.SerializeObject(submission);

        // Act
        var result = JsonConvert.DeserializeObject<AbstractSubmission>(serializedValue);

        // Assert
        result.Should().BeOfType<RegistrationSubmission>();
    }
}