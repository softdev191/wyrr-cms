//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PENPAL.DataStore
{
    using System;
    using System.Collections.Generic;
    
    public partial class trnUserParticipantEventDetail
    {
        public long Id { get; set; }
        public Nullable<long> EventId { get; set; }
        public Nullable<long> ParticipantUserId { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDateTime { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
        public Nullable<bool> IsPaymentDone { get; set; }
    
        public virtual trnUserParticipantEventDetail trnUserParticipantEventDetails1 { get; set; }
        public virtual trnUserParticipantEventDetail trnUserParticipantEventDetail1 { get; set; }
        public virtual trnUserEventDetail trnUserEventDetail { get; set; }
    }
}