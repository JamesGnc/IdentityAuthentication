using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IdentityAuthentication_Master.Models
{
    public class ResponseResult<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }

        public ResponseResult(T data, int code, string message)
        {
            Data = data;
            Code = code;
            Message = message;
        }

        public static ResponseResult<T> Success(T data, string message = "Success")
        {
            return new ResponseResult<T>(data, 0, message);
        }

        public static ResponseResult<T> SuccessList(T data, string message = "Success")
        {
            return new ResponseResult<T>(data, 0, message);
        }

        public static ResponseResult<T> Failure(T data, string message, int code = 1)
        {
            return new ResponseResult<T>(data, code, message);
        }
    }


    public class ResponseResult {
        public int Code { get; set; }
        public string? Message { get; set; }

        public ResponseResult(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public static ResponseResult Success(string message = "Success")
        {
            return new ResponseResult(0, message);
        }

        public static ResponseResult NotFound(string message = "Data Not Found")
        {
            return new ResponseResult(0, message);
        }

        public static ResponseResult ParamInvalid(string message = "Param Invalid")
        {
            return new ResponseResult(0, message);
        }

        public static ResponseResult BadRequest(string message, int code = 0)
        {
            return new ResponseResult(code, message);
        }

        public static ResponseResult Failure(string message, int code = 1)
        {
            return new ResponseResult(code, message);
        }
    }


    public class ResponseResultList<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }

        public ResponseResultList(T data, int code, string message, int totalCount = 0)
        {
            Data = data;
            Code = code;
            Message = message;
            TotalCount = totalCount;
        }

        public static ResponseResultList<T> Success(T data, string message = "Success")
        {
            return new ResponseResultList<T>(data, 0, message);
        }

        public static ResponseResultList<T> SuccessList(T data, int totalCount, string message = "Success")
        {
            return new ResponseResultList<T>(data, 0, message, totalCount);
        }

        public static ResponseResultList<T> Failure(string message, int code = 1)
        {
            return new ResponseResultList<T>(default!, code, message);
        }
    }

}
