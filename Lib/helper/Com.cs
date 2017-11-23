using Lib.core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Lib.helper
{
    /// <summary>
    /// 公共方法类
    /// </summary>
    public static class Com
    {
        /// <summary>
        /// 如果是null就new一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T NewIfNull<T>(object obj) where T : new()
        {
            if (obj is T data)
            {
                return data;
            }
            return new T();
        }

        public static void AssertNotNull<T>(T target, string err_msg)
        {
            Assert(target, x => x != null, err_msg);
        }

        /// <summary>
        /// 断言
        /// </summary>
        public static void Assert<T>(T target, Func<T, bool> condition, string err_msg)
        {
            Assert(target, condition, () => new Exception(err_msg ?? "默认的断言异常信息"));
        }

        /// <summary>
        /// 断言
        /// </summary>
        public static void Assert<T, E>(T target, Func<T, bool> condition, Func<E> err)
            where E : Exception
        {
            if (!condition.Invoke(target))
            {
                throw err.Invoke() ?? new Exception("你给老子一个空异常，是不是傻屌？");
            }
        }

        /// <summary>
        /// 第一个非空字符串
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string FirstPlumpStrOrNot(params string[] strs) =>
            ConvertHelper.NotNullList(strs).Where(x => ValidateHelper.IsPlumpString(x)).FirstOrDefault();

        /// <summary>
        /// 获取动态类型
        /// </summary>
        /// <returns></returns>
        public static dynamic DynamicObj() => new System.Dynamic.ExpandoObject();

        /// <summary>
        /// 尝试获取请求上下文
        /// </summary>
        /// <param name="_context"></param>
        /// <returns></returns>
        public static HttpContext TryGetContext(HttpContext _context)
        {
            if (_context != null) { return _context; }
            var context = System.Web.HttpContext.Current;

            return context ?? throw new Exception("无法获取上下文对象");
        }

        /// <summary>
        /// 把对象可读属性读出来变成字典
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ObjectToStringDict(object data)
        {
            return ObjectToDict_(data).ToDictionary(x => x.Key, x => ConvertHelper.GetString(x.Value));
        }

        /// <summary>
        /// 对象转为字典，value如果为null将自动转为dbnull
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ObjectToSqlParamsDict(object data)
        {
            return ObjectToDict_(data).ToDictionary(x => x.Key, x => x.Value ?? DBNull.Value);
        }

        /// <summary>
        /// 对象转为sql参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<T> ObjectToSqlParameters<T>(object data) where T : IDataParameter, new()
        {
            return ObjectToSqlParamsDict(data).Select(x => new T()
            {
                ParameterName = $"@{x.Key}",
                Value = x.Value
            }).ToList();
        }

        /// <summary>
        /// 读取对象的属性以及相应的值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ObjectToDict_(object data)
        {
            var dict = new Dictionary<string, object>();

            var props = data.GetType().GetProperties().Where(x => x.CanRead);
            foreach (var x in props)
            {
                dict[x.Name] = x.GetValue(data);
            }

            return dict;
        }

        /// <summary>
        /// 把URL后面的参数变成字典
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ExtractUrlParams(string url)
        {
            var dict = new Dictionary<string, string>();
            if (!ValidateHelper.IsPlumpString(url)) { return dict; }

            if (url.IndexOf('#') >= 0)
            {
                url = url.Split('#')[0];
            }

            var url_sp = url.Split('?');
            if (!new int[] { 1, 2 }.Contains(url_sp.Length)) { throw new Exception("多问号错误"); }
            var param_part = url_sp.Length == 1 ? url_sp[0] : url_sp[1];

            foreach (var p in param_part.Split('&'))
            {
                var sp = p.Split('=');
                if (!new int[] { 1, 2 }.Contains(sp.Length)) { throw new Exception("多等号错误"); }
                if (sp.Length == 1)
                {
                    dict[sp[0]] = string.Empty;
                }
                else
                {
                    dict[sp[0]] = sp[1];
                }
            }

            return dict.ToDictionary(x => x.Key, x => EncodingHelper.UrlDecode(x.Value));
        }

        /// <summary>
        /// 获取Exception深层次的错误
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<string> GetExceptionMsgList(Exception e)
        {
            var list = new List<string>();

            list.Add(e?.Message);
            list.Add(e?.InnerException?.Message);
            list.Add(e?.InnerException?.InnerException?.Message);
            list.Add(e?.InnerException?.InnerException?.InnerException?.Message);
            list.Add(e?.InnerException?.InnerException?.InnerException?.InnerException?.Message);

            list = list.Where(x => ValidateHelper.IsPlumpString(x)).Distinct().ToList();

            return list;
        }

        /// <summary>
        /// 获取深层的错误信息["msg1","msg2","msg3"]
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetExceptionMsgJson(Exception e) => JsonHelper.ObjectToJson(GetExceptionMsgList(e));

        /// <summary>
        /// 运行命令行
        /// </summary>
        /// <param name="cmds"></param>
        /// <returns></returns>
        public static string RunExec(params string[] cmds)
        {
            using (var p = new System.Diagnostics.Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                //要执行的程序名称
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                //可能接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;
                //由调用程序获取输出信息
                p.StartInfo.CreateNoWindow = true;
                //不显示程序窗口
                p.Start();//启动程序

                foreach (var cmd in cmds)
                {
                    //向CMD窗口发送输入信息：
                    p.StandardInput.WriteLine(cmd);
                }
                p.StandardInput.Flush();
                p.StandardInput.Close();

                //获取CMD窗口的输出信息：
                return p.StandardOutput.ReadToEnd();
            }
        }

        /// <summary>
        /// 包含在一个try catch的域里
        /// </summary>
        public static void TryScope(Action func, Action<Exception> onException = null, Action onFinally = null)
        {
            try
            {
                func.Invoke();
            }
            catch (Exception e)
            {
                onException?.Invoke(e);
            }
            finally
            {
                onFinally?.Invoke();
            }
        }

        /// <summary>
        /// 复制sql参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T[] CloneParams<T>(IEnumerable<T> list) where T : DbParameter
        {
            return list.Select(x => (T)((ICloneable)x).Clone()).ToArray();
        }

        /// <summary>
        /// 格式化数字格式
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FormatNumber(string num)
        {
            var sp = ConvertHelper.GetString(num).Split('.');
            if (sp.Length != 2) { return num; }
            var right = ConvertHelper.GetString(sp[1]);
            right = right.TrimEnd('0');
            var left = sp[0];
            if (ValidateHelper.IsPlumpString(right))
            {
                left += "." + right;
            }
            return left;
        }

        /// <summary>
        /// 格式化数字，获取xxx xxxk xxxw
        /// 测试通过
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string SimpleNumber(Int64 num)
        {
            if (num < 0) { throw new Exception("数字不能小于0"); }
            if (num < 1_000) { return num.ToString(); }
            if (num < 10_000)
            {
                return $"{(num / 1_000.0).ToString("0.0")}k";
            }
            return $"{(num / 1_0000.0).ToString("0.0")}w";
        }

        /// <summary>
        /// 格式化数字，比如（100000-100,000）
        /// </summary>
        /// <param name="num">需要格式化的数字</param>
        /// <returns></returns>
        public static string FormatNum(double num)
        {
            string re = string.Empty;
            char[] chars = num.ToString().ToArray();
            int cur = 0;
            for (int i = chars.Length - 1; i >= 0; --i)
            {
                ++cur;
                if (cur == 3 && i != 0)
                {
                    re = "," + chars[i] + re;
                    cur = 0;
                }
                else
                {
                    re = chars[i] + re;
                }
            }
            return re;
        }

        /// <summary>
        /// 兆转为字节
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int MbToB(float m) => (int)(m * 1024 * 1024);

        /// <summary>
        /// 通过密码生成一个key
        /// </summary>
        public static string GetPassKey(string pass, string salt = "密码盐")
        {
            if (!ValidateHelper.IsPlumpString(pass))
            {
                throw new Exception("密码为空");
            }
            var str = new StringBuilder();
            var list = (pass + salt).ToArray().Select(x => (int)x).ToList();
            str.Append(list.Count());
            str.Append(list.Min());
            str.Append(list.Sum());
            str.Append(list.Average());
            str.Append(list.Max());
            var sortlist = str.ToString().ToArray().Select(x => x.ToString()).ToList();
            sortlist.Sort(new MyStringComparer());
            return SecureHelper.GetMD5(string.Join("{*}", sortlist));
        }

        /// <summary>
        /// 设置请求ID
        /// </summary>
        public static void SetNewRequestID() => HttpContext.Current.Items["req_guid"] = GetUUID();

        /// <summary>
        /// 获取请求ID
        /// </summary>
        /// <returns></returns>
        public static string GetRequestID() => ConvertHelper.GetString(HttpContext.Current.Items["req_guid"]);

        /// <summary>
        /// 获取对象占用内存大小
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long GetByteSize(object obj)
        {
            if (obj == null) { return 0; }
            var formatter = new BinaryFormatter();

            using (var rems = new MemoryStream())
            {
                formatter.Serialize(rems, obj);
                return rems.Length;
            }
        }

        /// <summary>
        /// 获取唯一uuid标识
        /// </summary>
        /// <returns></returns>
        public static string GetUUID() => Guid.NewGuid().ToString().Replace("-", "").ToLower();

        /// <summary>
        /// 匿名方法的参数和你传入的res是一个对象，可以忽略直接使用res。
        /// 程序执行完成后res将被释放
        /// 如果执行错误会抛出异常，并且释放对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="res"></param>
        /// <param name="func"></param>
        public static void Using<T>(T res, Func<T, bool> func) where T : IDisposable
        {
            TryScope(() => { func.Invoke(res); }, (e) => { throw e; }, () => { res.Dispose(); });
        }

        /// <summary>
        /// 获取拼音
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        public static string Pinyin(string cn)
        {
            if (!ValidateHelper.IsPlumpString(cn))
            {
                return cn;
            }
            //定义拼音区编码数组
            var getValue = new int[]{
          -20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,
          -20032,-20026,-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,
          -19756,-19751,-19746,-19741,-19739,-19728,-19725,-19715,-19540,-19531,-19525,-19515,
          -19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263,-19261,-19249,
          -19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,
          -19003,-18996,-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,
          -18731,-18722,-18710,-18697,-18696,-18526,-18518,-18501,-18490,-18478,-18463,-18448,
          -18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, -18181,-18012,
          -17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,
          -17733,-17730,-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,
          -17468,-17454,-17433,-17427,-17417,-17202,-17185,-16983,-16970,-16942,-16915,-16733,
          -16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459,-16452,-16448,
          -16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,
          -16212,-16205,-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,
          -15933,-15920,-15915,-15903,-15889,-15878,-15707,-15701,-15681,-15667,-15661,-15659,
          -15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416,-15408,-15394,
          -15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,
          -15149,-15144,-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,
          -14941,-14937,-14933,-14930,-14929,-14928,-14926,-14922,-14921,-14914,-14908,-14902,
          -14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668,-14663,-14654,
          -14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,
          -14170,-14159,-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,
          -14109,-14099,-14097,-14094,-14092,-14090,-14087,-14083,-13917,-13914,-13910,-13907,
          -13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658,-13611,-13601,
          -13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,
          -13340,-13329,-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,
          -13068,-13063,-13060,-12888,-12875,-12871,-12860,-12858,-12852,-12849,-12838,-12831,
          -12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346,-12320,-12300,
          -12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,
          -11781,-11604,-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,
          -11055,-11052,-11045,-11041,-11038,-11024,-11020,-11019,-11018,-11014,-10838,-10832,
          -10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331,-10329,-10328,
          -10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254};
            //定义拼音数组
            var getName = new string[] {
          "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
          "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Cai","Can",
          "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
          "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
          "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
          "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
          "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
          "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
          "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
          "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
          "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
          "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
          "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
          "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
          "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
          "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
          "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
          "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
          "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
          "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
          "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
          "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
          "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
          "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
          "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
          "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
          "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
          "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
          "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
          "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
          "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
          "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
          "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"};

            var reg = new Regex("^[\u4e00-\u9fa5]$");//验证是否输入汉字
            byte[] arr = new byte[2];
            string pystr = "";
            int asc = 0, M1 = 0, M2 = 0;
            char[] mChar = cn.ToCharArray();//获取汉字对应的字符数组
            for (int j = 0; j < mChar.Length; j++)
            {
                //如果输入的是汉字
                if (reg.IsMatch(mChar[j].ToString()))
                {
                    arr = System.Text.Encoding.Default.GetBytes(mChar[j].ToString());
                    M1 = (short)(arr[0]);
                    M2 = (short)(arr[1]);
                    asc = M1 * 256 + M2 - 65536;
                    if (asc > 0 && asc < 160)
                    {
                        pystr += mChar[j];
                    }
                    else
                    {
                        switch (asc)
                        {
                            case -9254:
                                pystr += "Zhen"; break;
                            case -8985:
                                pystr += "Qian"; break;
                            case -5463:
                                pystr += "Jia"; break;
                            case -8274:
                                pystr += "Ge"; break;
                            case -5448:
                                pystr += "Ga"; break;
                            case -5447:
                                pystr += "La"; break;
                            case -4649:
                                pystr += "Chen"; break;
                            case -5436:
                                pystr += "Mao"; break;
                            case -5213:
                                pystr += "Mao"; break;
                            case -3597:
                                pystr += "Die"; break;
                            case -5659:
                                pystr += "Tian"; break;
                            default:
                                for (int i = (getValue.Length - 1); i >= 0; i--)
                                {
                                    if (getValue[i] <= asc)//判断汉字的拼音区编码是否在指定范围内
                                    {
                                        pystr += getName[i];//如果不超出范围则获取对应的拼音
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }
                else//如果不是汉字
                {
                    pystr += mChar[j].ToString();//如果不是汉字则返回
                }
            }
            return pystr;//返回获取到的汉字拼音
        }

        /// <summary>
        /// 取字符串拼音首字母
        /// </summary>
        public static string GetSpell(string value)
        {
            if (!ValidateHelper.IsPlumpString(value))
            {
                return value;
            }
            //27个
            int[] beginArr = new int[] {
            45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614,48119, 48119, 49062,
            49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698,
            52698, 52980, 53689, 54481, 55295 };
            //26个
            char[] characters = new char[] {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k','l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            string spell = string.Empty;
            byte[] b = null;
            value.ToList().ForEach(delegate (char c)
            {
                if ((b = Encoding.Default.GetBytes(c.ToString())) == null)
                {
                    spell += " ";
                    return;
                }
                if (b.Length == 1)
                {
                    spell += c;
                    return;
                }
                int code = (short)b[0] * 256 + (short)b[1];
                for (int i = 0; i < characters.Length; ++i)
                {
                    if (code >= beginArr[i] && code < beginArr[i + 1])
                    {
                        spell += characters[i];
                        return;//如果找到了就返回这个委托方法[如果是for循环要设置标志]
                    }
                }
                spell += " ";
            });
            return spell;
        }

        /// <summary>
        /// 生成缓存的key
        /// </summary>
        /// <param name="preStr"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string GetCacheKey(string preStr, params string[] ps)
        {
            if (ps == null) { ps = new string[] { }; }

            var list = new List<string>();
            var i = 0;
            foreach (var p in ps)
            {
                list.Add($"p{++i}={p}");
            }

            return preStr + ":" + string.Join(",", list);
        }

        /// <summary>
        /// 弹出索引最大的一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(ref List<T> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return default(T); }
            var obj = list[list.Count - 1];
            list.Remove(obj);
            return obj;
        }

        /// <summary>
        /// 获取交集(不返回null)
        /// </summary>
        public static List<T> GetInterSection<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            if (!ValidateHelper.IsPlumpList(a) || !ValidateHelper.IsPlumpList(b)) { return new List<T>(); }
            return a.Where(x => b.Contains(x)).ToList();
        }

        /// <summary>
        /// 多个集合里取交集(不返回null)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static List<T> GetInterSectionFromAny<T>(params List<T>[] lists)
        {
            if (!ValidateHelper.IsPlumpList(lists) || lists.Length < 2) { return new List<T>(); }
            var interSection = GetInterSection(lists[0], lists[1]);
            //原来这里i从3开始
            for (int i = 2; i < lists.Length; ++i)
            {
                //发现有非交集的就中断
                if (!ValidateHelper.IsPlumpList(interSection = GetInterSection(interSection, lists[i])))
                {
                    //原来这里如果没有找到焦急就break，最终返回一个空的list（interSection），现在直接返回
                    return new List<T>();
                }
            }
            return interSection;
        }

        /// <summary>
        /// 这里的ref十分必要，必须是引用到对象本身才可以
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap(ref int a, ref int b)
        {
            //bool eq = a << 32 == Math.Pow(2, 32) * a;
            a = a ^ b;
            b = a ^ b;
            a = a ^ b;
        }

        /// <summary>
        /// 这里的ref十分必要，必须是引用到对象本身才可以
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp;
            temp = b;
            b = a;
            a = temp;
        }

        /// <summary>
        /// 获取字符前几位，并加省略号
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetPreviewString(string str, int len)
        {
            str = ConvertHelper.GetString(str);
            return (str.Length <= len) ? str : str.Substring(0, len) + "...";
        }

        /// <summary>
        /// 获取随机数字
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetRandomNumString(int len = 10)
        {
            var str = new StringBuilder();
            var ran = new Random(DateTime.Now.Millisecond);
            while (true)
            {
                if ((--len) < 0) { break; }
                str.Append(ran.Next(9));
            }
            return str.ToString();
        }

        /// <summary>
        /// 排序
        /// </summary>
        public static T[] Sort<T>(T[] data, Func<T, T, bool> changePos)
        {
            for (int i = 0; i < data.Length - 1; ++i)
            {
                for (int j = 0; j < data.Length - 1 - i; ++j)
                {
                    if (changePos.Invoke(data[j], data[j + 1]))
                    {
                        Swap<T>(ref data[j], ref data[j + 1]);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 排序方法2
        /// </summary>
        public static T[] Sort_123<T>(T[] data, Func<T, T, bool> changePos)
        {
            for (int i = 0; i < data.Length - 1; ++i)
            {
                for (int j = i + 1; j < data.Length; ++j)
                {
                    if (changePos.Invoke(data[i], data[j]))
                    {
                        Swap<T>(ref data[i], ref data[j]);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 排序
        /// </summary>
        public static List<T> SortList<T>(List<T> data, Func<T, T, bool> changePos)
        {
            return Sort(data.ToArray(), changePos).ToList();
        }

        /// <summary>
        /// 字典变url格式(a=1&b=3)
        /// </summary>
        public static string DictToUrlParams(Dictionary<string, string> data)
        {
            if (!ValidateHelper.IsPlumpDict(data)) { return string.Empty; }
            var arr = data.Keys.Select(x => $"{x}={ConvertHelper.GetString(data[x])}");
            return string.Join("&", arr);
        }

        /// <summary>
        /// 反转list
        /// </summary>
        public static void ReversalList<T>(ref List<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            if (list.Count < 2) { return; }
            for (int i = 0; i < list.Count / 2; ++i)
            {
                var temp = list[i];
                var second_index = list.Count - 1 - i;

                list[i] = list[second_index];
                list[second_index] = temp;
            }
        }

        /// <summary>
        /// 通过空格 中英文逗号分隔（结果按照orderby排序）
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<string> SplitTags(string tags)
        {
            var list = StringHelper.Split(tags, ' ', ',', '，');
            if (!ValidateHelper.IsPlumpList(list))
            {
                return list;
            }
            return list.Where(x => ValidateHelper.IsLenInRange(x, 1, 30)).OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 过滤没用的tag
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string FormatTagsString(string tags) => string.Join(",", SplitTags(tags));

        /// <summary>
        /// 转全角的函数(SBC case)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSBC(string input)
        {
            //半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                {
                    c[i] = (char)(c[i] + 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        ///  转半角的函数(SBC case)
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns></returns>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }

        #region 历遍树算法
        /// <summary>
        /// 使用栈找文件
        /// </summary>
        public static void FindFiles(string path, VoidFunc<FileInfo> func, VoidFunc<int> stack_count_func = null)
        {
            var root = new DirectoryInfo(path);
            if (!root.Exists) { throw new Exception("目录不存在"); }

            var stack = new Stack<DirectoryInfo>();
            stack.Push(root);
            DirectoryInfo cur_node = null;
            while (stack.Count > 0)
            {
                stack_count_func?.Invoke(stack.Count);

                cur_node = stack.Pop();
                if (cur_node == null || !cur_node.Exists) { break; }

                var files = cur_node.GetFiles();
                if (ValidateHelper.IsPlumpList(files))
                {
                    files.ToList().ForEach(x => { func.Invoke(x); });
                }

                var dirs = cur_node.GetDirectories();
                if (ValidateHelper.IsPlumpList(dirs))
                {
                    dirs.ToList().ForEach(x => { stack.Push(x); });
                }
            }
        }

        /// <summary>
        /// 使用递归找文件目录
        /// </summary>
        public static void FindFilesBad(DirectoryInfo dir, VoidFunc<FileInfo> func)
        {
            if (dir == null || !dir.Exists) { return; }
            var files = dir.GetFiles();
            if (ValidateHelper.IsPlumpList(files))
            {
                files.ToList().ForEach(x => { func.Invoke(x); });
            }

            var dirs = dir.GetDirectories();
            if (ValidateHelper.IsPlumpList(dirs))
            {
                dirs.ToList().ForEach(x => { FindFilesBad(x, func); });
            }
        }
        #endregion

        /// <summary>
        /// 检查页码范围，并修正后返回
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static int? CheckPage(int? page)
        {
            page = page ?? 1;
            return page < 1 ? 1 : page;
        }

        /// <summary>
        /// 检查页面大小范围，并修正后返回
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int? CheckPageSize(int? size)
        {
            size = size ?? 10;
            return size < 1 ? 10 : size;
        }

        /// <summary>
        /// 监听目录文件变化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        public static void WatcherStrat(string path, string filter = "*.*")
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Filter = filter;
            watcher.Changed += (source, e) => { };
            //watcher.Created += new FileSystemEventHandler(OnProcess);
            //watcher.Deleted += new FileSystemEventHandler(OnProcess);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.IncludeSubdirectories = true;
        }

        /// <summary>
        /// 提取文本中的话题，格式：#话题#
        /// http://blog.csdn.net/hfut_jf/article/details/49745701
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string> FindTagsFromStr(string str)
        {
            var matchs = RegexHelper.FindMatchs(ConvertHelper.GetString(str), "#[^#]+#");
            var list = matchs?.Where(x => x.Success).Select(x => x.Value).Distinct().ToList();
            if (list == null) { list = new List<string>(); }
            return list;
        }

        /// <summary>
        /// 提取文本中@对象 @用户
        /// http://blog.csdn.net/hfut_jf/article/details/49745701
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string> FindAtFromStr(string str)
        {
            var matchs = RegexHelper.FindMatchs(ConvertHelper.GetString(str), "@[\u4e00-\u9fa5a-zA-Z0-9_-]{2,30}");
            var list = matchs?.Where(x => x.Success).Select(x => x.Value).Distinct().ToList();
            if (list == null) { list = new List<string>(); }
            return list;
        }

        /// <summary>
        /// 询价单有效期为30分钟，17.30到第二天早上9点停止报价，如果下午17.20发布询价单，其实是第二天早上才过期
        /// 从开始时间当天开始，计算每天能消耗的时间，不断迭代消耗，直到时间被消耗完
        /// </summary>
        public static DateTime GetExpireTime(DateTime createTime, double DurationSeconds, double day_start_hours = 9, double day_end_hours = 17)
        {
            //当天开始消耗时间的开始位置
            var time = createTime;
            var secondsLeft = DurationSeconds;
            while (true)
            {
                var start = time.Date.AddHours(day_start_hours);
                var end = time.Date.AddHours(day_end_hours);
                //今天实际开始计时的开始时间
                var jishikaishi = default(DateTime);
                if (time < start)
                {
                    jishikaishi = start;
                }
                else if (time > end)
                {
                    jishikaishi = end;
                }
                else
                {
                    jishikaishi = time;
                }
                //今天能消耗的时间
                var xiaohaoshijian = (end - jishikaishi).TotalSeconds;
                //能消耗的时间大于等于剩余的时间
                if (xiaohaoshijian >= secondsLeft)
                {
                    //返回最终时间
                    return jishikaishi.AddSeconds(secondsLeft);
                }
                //减去消耗时间
                secondsLeft -= xiaohaoshijian;
                //今天没能消耗所有时间，计算下一天
                time = time.Date.AddDays(1).AddHours(day_start_hours);
            }
        }

        /// <summary>
        /// 实现python中的range
        /// </summary>
        public static IEnumerable<int> Range(int a, int? b = null, int step = 1)
        {
            foreach (var i in RangeDouble(a, b, step))
            {
                yield return (int)i;
            }
        }

        /// <summary>
        /// 实现python中的range
        /// </summary>
        public static IEnumerable<double> RangeDouble(double a, double? b = null, double step = 1)
        {
            if (step == 0) { throw new Exception($"{nameof(step)}不可以为0"); }

            var start = default(double);
            var end = default(double);

            if (b == null)
            {
                start = 0;
                end = a;
            }
            else
            {
                start = a;
                end = b.Value;
            }

            for (var i = start; i < end; i = i + step)
            {
                yield return i;
            }
        }

        public static void testYield()
        {
            foreach (var i in Range(10, 78))
            {
                Console.WriteLine($"外部{i}");
            }
        }

    }
}