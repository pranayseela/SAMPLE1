using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER
{
    public class Msg
    {
        public static readonly string ERROR_LOGIN_LDAP_SERVER_INVALID = "Unable to login to LDAP server to validate user credentials.";
        public static readonly string ERROR_LOGIN_LDAP_USER_PRINCIPAL_INVALID = "Unable to retrieve user entry from LDAP.";

        public static readonly string ERROR_LOGIN_USER_NAME_REQUIRED = "Employee number and password are required";
        public static readonly string ERROR_LOGIN_USER_ACCOUNT_NOT_FOUND = "Employee user account not found in LDAP.";
        public static readonly string ERROR_LOGIN_USER_ACCOUNT_IS_NOT_ACTIVE = "Employee user account is not active in LDAP.";
        public static readonly string ERROR_LOGIN_USER_ACCOUNT_EXPIRED = "Employee user account has expired in LDAP.";
        public static readonly string ERROR_LOGIN_DISABLED = "Employee user is disabled in LDAP.";
        public static readonly string ERROR_LOGIN_PASSWORD_EXPIRED = "Your password has expired.";
        public static readonly string ERROR_LOGIN_FAIL_INVALID_CREDENTIALS = "The employee number or password is incorrect. Please try again.";

        public static readonly string ERROR_UNABLE_TO_ACCESS_APPLICATION_DATABASE = "Unable to access the application database.";
        public static readonly string ERROR_EMPLOYEE_NOT_FOUND_IN_APPLICATION = "The employee number does not exist in the application.";
        public static readonly string ERROR_EMPLOYEE_NOT_IN_TSE_ROLE = "The employee number does not exist in the TSE Crews group.";
        public static readonly string ERROR_EMPLOYEE_NOT_ACTIVE_IN_APPLICATION = "The employee account is not active in the application.";

        public static readonly string ERROR_OCCURRED_ON_SERVER = "An unexpected condition has occurred that is preventing the server from fulfilling the request.";
        public static readonly string ERROR_OCCURRED_ON_SERVER_SAVE_FAIL = "An unexpected event on the server is preventing the system from saving the information.";
        public static readonly string NO_DATA_FOUND_FOR_SELECTED_CRITERIA = "No records were found matching the selected filter criteria.";

        public static readonly string ERROR_CANNOT_CERTIFY = "Cannot certify hours of service.";
        public static readonly string ERROR_CANNOT_QUICKTIE = "Cannot save the quick-tie information.";
        public static readonly string ERROR_CANNOT_CALCULATE_HOS = "Cannot calculate hours of service.";
        public static readonly string ERROR_RELEASED_TIME_GREATER_THAN_SYSTEM_TIME = "Released time cannot be later than current time.";

        public static readonly string ERROR_VALIDATION_MESSAGE_INFO = "An error message is displayed near each field which is required or is incorrect.";
        public static readonly string ERROR_CORRECT_THE_ERRORS = "Please correct the errors and try again.";

        public static readonly string ERROR_CALCULATE_HOS = "Click \"CALCULATE\" to recalculate the hours of service.";

        public static readonly string INFO_PRIOR_JOB_ADDED = "Because the prior time off for the current job is less than 8 hours, the prior certified job has been added to the current tour of duty.";

        public static readonly string ERROR_JOB_CANNOT_START_BEFORE = "The job cannot start before {0}.";

        public static readonly string ERROR_VIOLATION_PHR = "Prior time off entered is less than the required mandatory rest of {0}";
        public static readonly string ERROR_VIOLATION_HOS = "Hours of service exceeds 12 hours.";

        public static readonly string ERROR_VIOLATION_COMMENTS_REQUIRED = "Comments required explaining the reason for violation.";
        public static readonly string ERROR_PHR_MISMATCH_COMMENTS_REQUIRED = "Comments required explaining why the prior time off does not match the system calculated value.";
        public static readonly string ERROR_BEGIN_END_LOCATION_MISMATCH_COMMENTS_REQUIRED = "Comments required explaining why the begin location does not match the end location.";
        public static readonly string ERROR_OTHER_LOCATION_COMMENT_REQUIRED = "Comments required explaining why location OTHER is selected.";
        public static readonly string ERROR_OTHER_TRANSIT_TYPE_COMMENT_REQUIRED = "Comments required explaining why transit type OTHER is selected.";

        public static readonly string ERROR_LOADING_FULLTIE_PAGE_FEED = "The server was not able to load the certification page for job {0} on {1}.";
        public static readonly string ERROR_LOADING_FULLTIE_PAGE_BOOK = "The server is not able to load the certification page for job {0} on {1}.";
        public static readonly string ERROR_LOADING_QUICKTIE_PAGE = "An unexpected condition has occurred on the server that is preventing the quick-tie page from loading.";

        public static readonly string ERROR_CERTIFICATION_REQUIRES_TSE_ON_DUTY = "Certification is allowed only during active tour of duty";
        public static readonly string ERROR_JOB_NOT_VALID_FOR_DAY_OF_WEEK = "The job {0} is not valid on {1} as per the current schedule";

        public static readonly string ERROR_PRIOR_REST_VIOLATION_NEXT_JOB = "The released time of this job causes a prior rest violation in the next certified record.";

        public static readonly string ERROR_ENTER_JOB_NUMBER = "Enter job number";
        public static readonly string ERROR_ENTER_VALID_LOCATION = "Enter a valid location";
        public static readonly string ERROR_ENTER_LOCATION = "Enter a location";
        public static readonly string ERROR_ENTER_TRAIN_NAME = "Enter train name";
        public static readonly string ERROR_ADD_TRAIN_TO_LIST = "Add train to list";
        public static readonly string ERROR_VALID_DATE_AND_TIME = "Enter a valid date & time";
        public static readonly string ERROR_TIME_IN_HOUR_MINUTES_FORMAT = "Enter time in hours:minutes format";
        public static readonly string ERROR_VALID_TIME_IN_HOUR_MINUTES_FORMAT = "Enter a valid time in hours:minutes format";
        public static readonly string ERROR_BEGIN_LOCATION_SAME_AS_END_LOCATION = "Begin location is same as end location";
        public static readonly string ERROR_BEGIN_TIME_LATER_THAN_END_TIME = "Begin time is later than end time";
        public static readonly string ERROR_SELECT_TRANSIT_MODE = "Select a transit mode";

        public static readonly string WARN_UNCERTIFIED_RECORDS_EXCEED_THRESHOLD = "There are {0} or more uncertified records";

        public static readonly string INFO_PASSWORD_EXPIRY_REMINDER_MESSAGE = @"<ol><li>Your password will expire on <b><span style=""color:#c00"">{0}</span></b>. Select <span style=""color:#c00"">Change Password</span> below at any time to change your password. </li>
                                                                                <li>Going to the LIRR Password Portal will end your current eHOS session, complete any open records before leaving the application. </li>
                                                                                <li>You will then log into eHOS using your new password.</li></ol>";
        public static readonly string INFO_PASSWORD_EXPIRD_MESSAGE = "Select <em>Reset Password</em> to navigate to the LIRR Password Portal.";
    }
}