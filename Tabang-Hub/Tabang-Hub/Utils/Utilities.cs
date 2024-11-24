using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Utils
{
    public enum ErrorCode
    {
        Success,
        Error
    }
    public enum Status
    {
        InActive,
        Active
    }

    public enum RoleType
    {
        Volunteer,
        Organization
    }

    public enum ProductStatus
    {
        NoStock,
        HasStock
    }

    public enum OrderStatus
    {
        Open,
        Pending,
        Paid,
        Delivered,
        Close
    }

    public class Constant
    {
        public const string Role_Customer = "Volunteer";
        public const string Role_Admin = "Organization";

        public const int ERROR = 1;
        public const int SUCCESS = 0;

        public const string X = "X";
        public const string MINUS = "−";
        public const string PLUS = "+";
    }

    public class Utilities
    {
        public static String gUid
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }
    }
}