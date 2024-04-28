using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using POC_API.sms_campaign;
using Telnyx;


namespace POC_API.SMS
{
    public class SmsService
    {
        private readonly ApplicationDbContext _dbContext;

        public SmsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> SendMultipleSmsAsync(string message, string Campaign_name)
        {


            try
            {
                var allCamps = _dbContext.campaignTable.Where(u => u.campaignName.Equals(Campaign_name)).Any();

                if (allCamps)
                {
                    return "This campaign already exits";
                }

                var campainData = new campaignTable
                {
                    campaignName = Campaign_name,
                    message = message,
                    dateTime = DateTime.Now
                };
                _dbContext.campaignTable.Add(campainData);
                
                await _dbContext.SaveChangesAsync();

                var camp = _dbContext.campaignTable.Where(x => x.campaignName == Campaign_name).FirstOrDefault();

                var userData = await _dbContext.userDatas
                    .Where(u => u.IsActive == 1)
                    .ToListAsync();

                var campData = await _dbContext.SmsCampaigns.ToListAsync();

                var messageService = new MessageService();


                foreach (var user in userData) {

                    if (campData.FindAll(x => x.userid == user.Id).Any()) {
                        continue;
                    }

                    var newMessage = new NewMessage
                    {
                        From = "+14433923316", // Your Telnyx phone number
                        To = "+" + user.Number,
                        Text = message,
                    };

                    await messageService.CreateAsync(newMessage);

                    var campaign = new SmsCampaigns
                    {
                        userid = user.Id,
                        campaignId = camp.Id,
                        //ThreadId = 
                    };
                    _dbContext.SmsCampaigns.Add(campaign);
                    await _dbContext.SaveChangesAsync();

                }

                return "";
            }
            catch (TelnyxException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


    }

}
