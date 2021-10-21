using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.DataProviders
{
    public class EventManagementProvider
    {

        public static WYRREntities db = new WYRREntities();

        public ResultModelWithID AddEventDetails(EventModel Event)
        {
            ResultModelWithID result = new ResultModelWithID();

            try
            {
                
                using (var context = new WYRREntities())
                {
                    #region AddUserEvent

                    trnUserEventDetail objEventDetails = new trnUserEventDetail();
                    objEventDetails.EventName = Event.EventName;
                    objEventDetails.Amount = Event.Amount;
                    objEventDetails.Currency = Event.Currency;
                    objEventDetails.IsDeleted = false;
                    objEventDetails.UserId = Event.UserID;
                    objEventDetails.CreatedDate = DateTime.UtcNow;
                    objEventDetails.UpdatedDate = DateTime.UtcNow;
                    context.trnUserEventDetails.Add(objEventDetails);
                    context.SaveChanges();
                    result.IsSuccess = true;
                    result.RecordID = Convert.ToInt32(objEventDetails.Id);

                    foreach (var item in Event.Participantslist)
                    {
                        trnUserParticipantEventDetail objParticipantEventDetail = new trnUserParticipantEventDetail();
                        objParticipantEventDetail.EventId = result.RecordID;
                        objParticipantEventDetail.ParticipantUserId = item.UserID;
                        objParticipantEventDetail.IsDeleted = false;
                        objParticipantEventDetail.IsPaymentDone = false;
                        objParticipantEventDetail.CreatedDateTime = DateTime.UtcNow;
                        objParticipantEventDetail.ModifiedDateTime = DateTime.UtcNow;
                        context.trnUserParticipantEventDetails.Add(objParticipantEventDetail);
                        context.SaveChanges();

                    }

                    #endregion

                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }


    }
}
