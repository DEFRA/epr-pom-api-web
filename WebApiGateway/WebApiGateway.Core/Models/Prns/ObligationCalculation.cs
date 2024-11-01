namespace WebApiGateway.Core.Models.Prns
{
    public class ObligationCalculation
    {
        public int OrganisationId { get; set; }

        public string MaterialName { get; set; }

        public int MaterialObligationValue { get; set; }

        public int Year { get; set; }

        public DateTime CalculatedOn { get; set; }

        public double MaterialWeight { get; set; }
    }
}