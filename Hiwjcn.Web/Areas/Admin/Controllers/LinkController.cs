using Hiwjcn.Bll;
using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Web.Models.Link;
using Lib.extension;
using Lib.helper;
using Lib.io;
using Lib.mvc;
using Lib.storage;
using Model.Sys;
using System;
using System.Web.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    public class LinkController : WebCore.MvcLib.Controller.UserBaseController
    {
        private ILinkService _ILinkService { get; set; }

        public LinkController(ILinkService link)
        {
            this._ILinkService = link;
        }

        /// <summary>
        /// 添加或者更新连接
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="link"></param>
        /// <param name="title"></param>
        /// <param name="image"></param>
        /// <param name="target"></param>
        /// <param name="order"></param>
        /// <param name="link_type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveLinkAction(int? id,
            string name, string link, string title, string image,
            string target, int? order, string link_type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                id = id ?? 0;
                order = order ?? -1;

                var model = new LinkModel();
                model.IID = id.Value;
                model.Name = name;
                model.Url = link;
                model.Title = title;
                model.Image = image;
                model.Target = target;
                model.OrderNum = order.Value;
                model.LinkType = link_type;
                if (!ValidateHelper.IsPlumpString(model.Url))
                {
                    model.Url = this.X.BaseUrl;
                }

                var res = string.Empty;

                if (id > 0)
                {
                    //update
                    res = _ILinkService.UpdateLink(model);
                }
                else
                {
                    //add
                    model.UserID = loginuser.UserID;
                    res = _ILinkService.AddLink(model);
                }

                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 删除链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteLinkAction(string id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var res = _ILinkService.DeleteLink(id);
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 处理上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FileUploadAction()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                string SavePath = Server.MapPath("~/static/upload/link_images/");

                var uploader = new FileUpload()
                {
                    AllowFileType = new string[] { ".gif", ".png", ".jpg", ".jpeg", ".bmp" },
                };
                var file = this.X.context.Request.Files["file"];
                if (file != null)
                {
                    var model = uploader.UploadSingleFile(file, SavePath);
                    var path = model.FilePath;
                    if (model.SuccessUpload && System.IO.File.Exists(path))
                    {
                        try
                        {
                            var url = QiniuHelper.Upload(path, Com.GetUUID());

                            ViewData["img_url"] = url;
                            //删除本地
                            System.IO.File.Delete(path);
                        }
                        catch (Exception e)
                        {
                            e.AddLog(this.GetType());
                            ViewData["info"] = "上传到七牛错误";
                        }
                    }
                    else
                    {
                        ViewData["info"] = model.Info;
                    }
                }
                return View();
            });
        }

        /// <summary>
        /// 编辑连接表单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult LinkEdit(string id, string link_type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new LinkEditViewModel();

                if (ValidateHelper.IsPlumpString(id))
                {
                    var link = _ILinkService.GetLinkByID(id);
                    if (link != null)
                    {
                        link_type = link.LinkType;
                    }
                    model.Link = link;
                }
                if (!ValidateHelper.IsLenInRange(link_type, 1, 30))
                {
                    return Content("缺少连接类型");
                }
                model.LinkType = link_type;

                ViewData["model"] = model;
                return View();
            });
        }

        /// <summary>
        /// 链接管理
        /// </summary>
        /// <returns></returns>
        public ActionResult LinkManage(string link_type)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (!ValidateHelper.IsPlumpString(link_type))
                {
                    return new GoHomeResult();
                }
                var model = new LinkListViewModel();

                model.LinkType = link_type;
                model.LinkList = _ILinkService.GetTopLinks(link_type, 1000);
                ViewData["model"] = model;
                return View();
            });
        }

    }
}
