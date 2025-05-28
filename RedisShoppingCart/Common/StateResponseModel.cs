using Newtonsoft.Json.Linq;

namespace Server.API.Common
{
    public class StateResponseModel<T>
    {
        public bool success { get; set; }     
        private string? message; // field
        public string Message   // property
        {
            get { return string.IsNullOrEmpty(message) ? "Thất bại" : message; }
            set { message = value; }
        }

        public T? data { get; set; }
        public static StateResponseModel<T> Success(T? value = default)
        {
            return new StateResponseModel<T>() { success = true, data = value , message = "Thành công" };
        }
        public static StateResponseModel<T> Error(string? msg = null)
        {
            return new StateResponseModel<T>() { success = false, data = default, message = msg };
        }
    }
}
