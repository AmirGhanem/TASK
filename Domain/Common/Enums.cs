using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public enum ErrorCode : byte
    {
        Error = 1,
        FieldRequired,
        MaxFieldLength,
        MinFieldLength,
        UserNotFound,
        InvalidLogin,
        DuplicateUser,
        DuplicateEmail,
        DuplicateName,
        DuplicateCRN,
        DuplicatePhoneNumber,
        NotFound,
        UserAlreadyLoggedIn,
        InvalidEmailAddress,
        InvalidPhoneNumber,
        InvalidUsername,
        InvalidPasswordRequirements,
        Expired,
        CantLockYourself,
        BadFile,
        InvalidQTY,
        InvalidPrice,
        InvalidStatus,
        InvalidOperation,
    }

    public enum UserType : byte
    {
        Admin = 1,
        User,
    }
    
    public enum OrderStatus : byte
    {
        Pending = 1,
        Preparing,
        Shipped,
        Delivered,
    }

}
