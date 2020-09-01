using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Linq
{
    /// <summary>
    /// 委托扩展类。
    /// </summary>
    public static class DelegateExtension
    {
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        public static void ExecuteRetry(this Action action, int retryCount)
        {
            int errCount = 0;
            do
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    errCount++;
                }

            } while (errCount < retryCount);
        }
        /// <summary>
        /// 执行委托，并指定重试次数和每次重试的间隔时间。
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="sleep">每次重试间隔的时间。</param>
        public static void ExecuteRetry(this Action action, int retryCount, int sleep = 1000)
        {
            int errCount = 0;
            do
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    errCount++;
                }
                System.Threading.Thread.Sleep(sleep);
            } while (errCount < retryCount);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static void ExecuteRetry(this Action action, int retryCount, Action<Exception, int> exceptionAction)
        {
            action.ExecuteRetry<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static void ExecuteRetry(this Action action, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            action.ExecuteRetry<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static void ExecuteRetry<T>(this Action action, int retryCount, Action<T, int> exceptionAction)
            where T : Exception
        {
            int errCount = 0;
            do
            {
                try
                {
                    action();
                    break;
                }
                catch (T ex)
                {
                    errCount++;
                    exceptionAction(ex, errCount);
                }
            } while (errCount < retryCount);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static void ExecuteRetry<T>(this Action action, int retryCount, Func<T, int, bool> exceptionAction)
            where T : Exception
        {
            int errCount = 0;
            do
            {
                try
                {
                    action();
                    break;
                }
                catch (T ex)
                {
                    errCount++;
                    if (exceptionAction(ex, errCount))
                    {
                        break;
                    }
                }
            } while (errCount < retryCount);
        }
#if NET40
        /// <summary>
        /// 异步执行委托，并指定重试次数和每次重试的间隔时间。
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="sleep">每次重试间隔的时间。</param>
        /// <returns></returns>
        public static Task ExecuteRetryAsync(this Action action, int retryCount, int sleep = 1000)
        {
            Task task = new Task(() =>
            {
                action.ExecuteRetry(retryCount, sleep);
            }, TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }
        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static Task ExecuteRetryAsync(this Action action, int retryCount, Action<Exception, int> exceptionAction)
        {
            return action.ExecuteRetryAsync<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static Task ExecuteRetryAsync(this Action action, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            return action.ExecuteRetryAsync<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static Task ExecuteRetryAsync<T>(this Action action, int retryCount, Action<T, int> exceptionAction)
            where T : Exception
        {
            Task task = new Task(() =>
            {
                action.ExecuteRetry<T>(retryCount, exceptionAction);
            }, TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static Task ExecuteRetryAsync<T>(this Action action, int retryCount, Func<T, int, bool> exceptionAction)
            where T : Exception
        {
            Task task = new Task(() =>
            {
                action.ExecuteRetry<T>(retryCount, exceptionAction);
            }, TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }
#else
        /// <summary>
        /// 异步执行委托，并指定重试次数和每次重试的间隔时间。
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="sleep">每次重试间隔的时间。</param>
        /// <returns></returns>
        public static async Task ExecuteRetryAsync(this Action action, int retryCount, int sleep = 1000)
        {
            await Task.Factory.StartNew(() =>
            {
                action.ExecuteRetry(retryCount, sleep);
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static async Task ExecuteRetryAsync(this Action action, int retryCount, Action<Exception, int> exceptionAction)
        {
            await action.ExecuteRetryAsync<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static async Task ExecuteRetryAsync(this Action action, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            await action.ExecuteRetryAsync<Exception>(retryCount, exceptionAction);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托</param>
        public static async Task ExecuteRetryAsync<T>(this Action action, int retryCount, Action<T, int> exceptionAction)
            where T : Exception
        {
            await Task.Factory.StartNew(() =>
            {
                action.ExecuteRetry<T>(retryCount, exceptionAction);
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="T">异常</typeparam>
        /// <param name="action">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static async Task ExecuteRetryAsync<T>(this Action action, int retryCount, Func<T, int, bool> exceptionAction)
            where T : Exception
        {
            await Task.Factory.StartNew(() =>
            {
                action.ExecuteRetry<T>(retryCount, exceptionAction);
            }, TaskCreationOptions.LongRunning);
        }
#endif
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retryCount"></param>
        public static Result<TResult> ExecuteRetry<TResult>(this Func<TResult> func, int retryCount)
        {
            int errCount = 0;
            do
            {
                try
                {
                    return func();
                }
                catch
                {
                    errCount++;
                }

            } while (errCount < retryCount);
            return new Result<TResult>() { Code = -1, Success = false, Message = $"错误次数到达最大次数{retryCount}" };
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static Result<TResult> ExecuteRetry<TResult>(this Func<TResult> func, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            int errCount = 0;
            do
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    errCount++;
                    if (exceptionAction(ex, errCount))
                    {
                        return new Result<TResult>() { Code = -1, Success = false, Message = $"错误{retryCount}次后关闭。" };
                    }
                }
            } while (errCount < retryCount);
            return new Result<TResult>() { Code = -1, Success = false, Message = $"错误次数到达最大次数{retryCount}" };
        }
        /// <summary>
        /// 执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static Result<TResult> ExecuteRetry<TResult>(this Func<TResult> func, int retryCount, Action<Exception, int> exceptionAction)
        {
            int errCount = 0;
            do
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    errCount++;
                    exceptionAction(ex, errCount);
                }
            } while (errCount < retryCount);
            return new Result<TResult>() { Code = -1, Success = false, Message = $"错误次数到达最大次数{retryCount}" };
        }
#if NET40
        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static Task<Result<TResult>> ExecuteRetryAsync<TResult>(this Func<TResult> func, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            Task<Result<TResult>> task = new Task<Result<TResult>>(() => func.ExecuteRetry(retryCount, exceptionAction), TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }
        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static Task<Result<TResult>> ExecuteRetryAsync<TResult>(this Func<TResult> func, int retryCount, Action<Exception, int>  exceptionAction)
        {
            Task<Result<TResult>> task = new Task<Result<TResult>>(() => func.ExecuteRetry(retryCount, exceptionAction), TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }
#else
        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static async Task<Result<TResult>> ExecuteRetryAsync<TResult>(this Func<TResult> func, int retryCount, Func<Exception, int, bool> exceptionAction)
        {
            return await Task.Factory.StartNew(() => func.ExecuteRetry(retryCount, exceptionAction), TaskCreationOptions.LongRunning); ;
        }
        /// <summary>
        /// 异步执行委托，并指定重试次数
        /// </summary>
        /// <typeparam name="TResult">返回的类型</typeparam>
        /// <param name="func">要执行的委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exceptionAction">执行异常时发生的委托,如果返回true,将结束重试</param>
        public static async Task<Result<TResult>> ExecuteRetryAsync<TResult>(this Func<TResult> func, int retryCount, Action<Exception, int> exceptionAction)
        {
            return await Task.Factory.StartNew(() => func.ExecuteRetry(retryCount, exceptionAction), TaskCreationOptions.LongRunning); ;
        }
#endif
    }
}
