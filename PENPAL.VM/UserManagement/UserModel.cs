using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class UserModel
    {
        public long UserID { get; set; }
        [Required(ErrorMessage = "Please enter user name")]
        public string UserName { get; set; }
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        [MaxLength(50)]
        public string EmailAddress { get; set; }
        [MaxLength(15)]
        //[Required(ErrorMessage = "Please enter user phone number")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter valid phone number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
        public string DefaultCurrency { get; set; }
        public Nullable<bool> IsSocialLogin { get; set; }
        public string ProfileImage { get; set; }
        public string UniqueUserId { get; set; }
        public string CountryCode { get; set; }


    }


    public class UserCMSModel
    {
        public long UserID { get; set; }
        [Required(ErrorMessage = "Please enter user name")]
        public string UserName { get; set; }
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        [MaxLength(50)]
        public string EmailAddress { get; set; }
        [MaxLength(15)]
        [Required(ErrorMessage = "Please enter user phone number")]
        //[RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter valid phone number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
        public string PublicKey { get; set; }
        public string DefaultCurrency { get; set; }
        public Nullable<bool> IsSocialLogin { get; set; }
        public string ProfileImage { get; set; }
        public string UniqueUserId { get; set; }
        public string CountryCode { get; set; }
        public bool? IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Updateddatetime { get; set; }
        public string LastLoginDateTime { get; set; }
      
    }

    public class MasterUserModel
    {
        public long ID { get; set; }
        [Required(ErrorMessage = "Please enter user public key")]
        public string PublicKey { get; set; }
        [Required(ErrorMessage = "Please enter user secret key")]
        public string SecretKey { get; set; }

    }




    public class UserAnalysisModel
    {
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Date { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string DefaultCurrency { get; set; }
        public Nullable<bool> IsSocialLogin { get; set; }
        public string ProfileImage { get; set; }
        public string UniqueUserId { get; set; }
        public string AccountNumber { get; set; }
        public string LastLoginDateTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Updateddatetime { get; set; }
    }


    public class ResetPassword
    {
        public long UserID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Old Password")]
        public string Password { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,16}", ErrorMessage = "The {0} field must be Minimum 8 and Maximum 16 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character($, @, !, %, *, ?, &).")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8)]
        [DisplayName("Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The New Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; }
    }



    public class CustomerLoginModel
    {
        [Required(ErrorMessage = "Please enter customer phone number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
    }

    public class ContactDetails
    {
        public string MobileNumber { get; set; }
        public string ProfileImage { get; set; }

    }

    public class UpdateUserModel
    {
        public int UserID { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    public class UpdateUserPasswordModel
    {
        public string MobileNumber { get; set; }
        public string Password { get; set; }

    }

    public class UserEmailModel
    {
        public string EmailId { get; set; }
    }

    public class UserContactModel
    {
        public int UserID { get; set; }
        public int PageNumber { get; set; }

    }

    public class UserContactModel_V2
    {
        public int UserID { get; set; }
        public int PageNumber { get; set; }
        public bool IsFrequentChat { get; set; }
    }


    public class SearchedUserContactModel
    {
        public int UserID { get; set; }
        public string SearchedText { get; set; }
    }

    public class SearchedUserModel
    {
        public long UserID { get; set; }
        public string SearchedText { get; set; }
    }

    public class UserWalletModel
    {
        public string AccountNumber { get; set; }
    }

    public class UserDetailsBeforeTransaction
    {
        public string AccountNumber { get; set; }
        public string Amount { get; set; }
        public int ReceiverUserID { get; set; }

    }




    public class UserLoginModel
    {
        public string Phonenumber { get; set; }
        public string Password { get; set; }
        public bool isEmail { get; set; }

    }

    public class UserAccountVerficationModel
    {
        public int UserId { get; set; }
        public bool IsLinkStellarAccount { get; set; }
        public string Publickey { get; set; }
        public string Secretkey { get; set; }
        public string OTP { get; set; }
    }

    public class UserOTPModel
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class UserVerifyOTPModel
    {
        public int UserId { get; set; }
        public string OTP { get; set; }
    }


    public class InviteUserModel
    {
        public long SenderID { get; set; }
        public long ReceiverID { get; set; }
    }

    public class InsertUpdateResponseModel
    {
        public string response { get; set; }
    }

    public class ChatUserModel
    {
        public long UserID { get; set; }
    }


    public class ChatInviteStatus
    {
        public long ConnectionID { get; set; }
        public string ConnectStatus { get; set; }
    }

    public class newsFeedModel
    {
        public long User_feedID { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool isApproved { get; set; }
        public DateTime? created_at { get; set; }
        public long? created_by { get; set; }
        public DateTime? modified_at { get; set; }
        public long? modified_by { get; set; }

    }

    public class newsFeedModelCMS
    {
        public long User_feedID { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool isApproved { get; set; }
        public string created_at { get; set; }
        public long? created_by { get; set; }
        public string modified_at { get; set; }
        public long? modified_by { get; set; }

    }

    public class newsUserIDFeedIDModel
    {
        public long User_feedID { get; set; }
    }
}
