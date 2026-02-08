using OpaqueTokenSample.Infrastructure.Cache.Helper;

using System;
using System.Collections.Generic;
using System.Text;

namespace OpaqueTokenSample.Security.Helper;

public static class ResponseHelper
{
    public static ResponseBase<TResult> ErrorResponse<TResult>(string errorMessage, ResultEnum resultType = ResultEnum.Error)
        where TResult : class
    {
        var response = new ResponseBase<TResult>(errorMessage)
        {
            status = resultType
        };
        return response;
    }
    public static ResponseBase<TResult> ErrorResponse<TResult>(List<string> errorMessages, ResultEnum resultType = ResultEnum.Warning)
    where TResult : class
    {
        var response = new ResponseBase<TResult>
        {
            messageList = errorMessages,
            status = resultType
        };
        return response;
    }
    public static ResponseBase<TResult> SuccessResponse<TResult>(this TResult data, ResultEnum resultType = ResultEnum.Success)
        where TResult : class
    {
        var response = new ResponseBase<TResult>
        {
            status = resultType,
            data = data
        };
        return response;
    }
    public static ResponseBase<TResult> SuccessResponse<TResult>(ResultEnum resultType = ResultEnum.Success)
        where TResult : class
    {
        var response = new ResponseBase<TResult>();
        response.status = resultType;
        return response;
    }
}
