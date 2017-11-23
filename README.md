### Asp.Net MVC开发框架
	>基于autofac的ioc依赖注入
	>基于autofac.dynamicproxy的aop面向切面编程
	>entity framework orm+仓储模式架构
	>events事件发布订阅
	>使用PreApplicationStartMethod实现的插件机制（不稳定）
	>基于json字典库的多语言
	>基于redis/内存的缓存组件
	>基于quartz的后台任务
	>基于polly的熔断，重试，超时etc
	>rabbitmq消息队列
	>elasticsearch检索
	>WCF调用封装
	>Mysql存储Session
	>第三方api调用
	>身份验证权限
	>Auth授权系统
	>Akka Actor编程模型
	>WebSocket（未完成）
	>各种类库...
	
### Lib项目
https://www.nuget.org/packages/lib.com.nuget/

![Lib项目](http://hiwjcn.qiniudn.com/tools.png)

### Bug反馈
issue or hiwjcn@live.com

### 约定

层级 | 引用 | Example
------------ | ------------- | -------------
data | `model` `dll`| `T_User_Info` `Lib`
service | `IRepository<>` `IData`|`IRepository<T_User_Info>` `IUserData`
controller | `IService`|`IUserService`

### 结构
```c#
    //数据层
    public interface IUserData : IRepository<T_User_Info>
    {
    }
    public class UserData : EFRepository<T_User_Info>, IUserData
    {
    }
    //业务层
    public interface IUserService : IServiceBase<T_User_Info>
    {
        T_User_Info GetByID(int id);
        List<T_User_Info> GetTop10();
        Tuple<bool, string> AddUser(T_User_Info model);
        Tuple<int, string> UpdateUser(T_User_Info model);
    }
    public class UserService : ServiceBase<T_User_Info>, IUserService
    {
        private readonly IUserData _IUserData;
        private readonly IEventPublisher _Publisher;
        private readonly IRepository<T_User_PayBill> _userpaybillRepository;
	
	//控制器注入实现
        public UserService(
            IUserData userdata,
            IEventPublisher publisher,
            IRepository<T_User_PayBill> userpaybillRepository)
        {
            this._IUserData = userdata;
            this._Publisher = publisher;
            this._userpaybillRepository = userpaybillRepository;

            this._Publisher.Publish("发布消息");
        }
        //methods
    }
    //控制器
    public class UserController : BaseController
    {
        private readonly IUserService _IUserService;
        private readonly IValidateCodeService _IValidateCodeService;

        public UserController(
            IUserService _IUserService,
            IValidateCodeService _IValidateCodeService)
        {
            this._IUserService = _IUserService;
            this._IValidateCodeService = _IValidateCodeService;
        }
    }
```
### 事件发布
```c#
    //实现消费接口
    public class UserEventHandler :
        IConsumer<EntityDeleted<T_User_Info>>,
        IConsumer<EntityUpdated<T_User_Info>>,
        IConsumer<EntityInserted<T_User_Info>>,
        IConsumer<string>
    {
        public void HandleEvent(EntityInserted<T_User_Info> eventMessage)
        {
            $"插入：{eventMessage.Entity.ToJson()}".AddBusinessInfoLog();
        }

        public void HandleEvent(string eventMessage)
        {
            $"来自订阅的消息：{eventMessage}".AddBusinessInfoLog();
        }

        public void HandleEvent(EntityUpdated<T_User_Info> eventMessage)
        {
            $"修改：{eventMessage.Entity.ToJson()}".AddBusinessInfoLog();
        }

        public void HandleEvent(EntityDeleted<T_User_Info> eventMessage)
        {
            $"删除：{eventMessage.Entity.ToJson()}".AddBusinessInfoLog();
        }
    }
    //发布消息
    //自动去ioc容器中找到实现并调用
    this._Publisher.Publish("发布消息");
```
### Aop
```c#
    //实现拦截接口，并注册到ioc中
    public class LogPerformance : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            try
            {
                var start = DateTime.Now;

                invocation.Proceed();

                var sec = (DateTime.Now - start).TotalSeconds;

                var name = invocation.Method.Name;
                $"{name}运行耗时：{sec}秒".AddBusinessInfoLog();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
    //[Intercept(typeof(LogPerformance))]添加到需要拦截的类，然后会自动拦截virtual方法
```
