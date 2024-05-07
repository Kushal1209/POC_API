namespace POC_API.sms_campaign
{
    public class SmsCampaigns
    {
        public int Id { get; set; }
        public int userid { get; set; }
        public int campaignId { get; set; }
        public string threadId { get; set; }
        public bool isConverted { get; set; }


        public SmsCampaigns()
        {
            isConverted = false;
            threadId = "0";
        }
    }


}
