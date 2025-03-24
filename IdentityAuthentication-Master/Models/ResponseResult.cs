namespace IdentityAuthentication_Master.Models
{
    public class ResponseResult<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }

        public ResponseResult(T data, int code, string message, int totalCount = 0)
        {
            Data = data;
            Code = code;
            Message = message;
            TotalCount = totalCount;
        }

        public static ResponseResult<T> Success(T data, string message = "Success")
        {
            return new ResponseResult<T>(data, 0, message);
        }

        public static ResponseResult<T> SuccessList(T data, int totalCount, string message = "Success")
        {
            return new ResponseResult<T>(data, 0, message, totalCount);
        }

        public static ResponseResult<T> Failure(string message, int code = 1)
        {
            return new ResponseResult<T>(default!, code, message);
        }
    }

}
