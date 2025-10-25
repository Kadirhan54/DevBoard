using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Dtos
{
    public class ResultDto<T> 
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ResultDto(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

}
