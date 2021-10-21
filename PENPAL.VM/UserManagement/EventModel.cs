using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class EventModel
    {
        public string EventName { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public int UserID { get; set; }
        public List<RecipientModel> Participantslist { get; set; }

    }

    public class RecipientModel
    {
        public int UserID { get; set; }
        public string Name { get; set; }


    }




}
