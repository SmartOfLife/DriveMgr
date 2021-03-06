﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DriveMgr;
using DriveMgr.BLL;

namespace DriveMgr.WebUI.admin.FinancialMgr
{
    /// <summary>
    /// bg_siteRental_payHandler 的摘要说明
    /// </summary>
    public class bg_siteRental_payHandler : IHttpHandler
    {

        DriveMgr.Model.UserOperateLog userOperateLog = null;   //操作日志对象
        SiteRentalLocalBLL siteRentalBll = new SiteRentalLocalBLL();
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            string action = context.Request.Params["action"];

            try
            {
                DriveMgr.Model.User userFromCookie = DriveMgr.Common.UserHelper.GetUser(context);   //获取cookie里的用户对象
                userOperateLog = new Model.UserOperateLog();
                userOperateLog.UserIp = context.Request.UserHostAddress;
                userOperateLog.UserName = userFromCookie.UserId;
                switch (action)
                {
                    case "add":
                        AddSiteRental(userFromCookie, context);
                        break;
                    case "edit":
                        EditSiteRental(userFromCookie, context);
                        break;
                    case "delete":
                        DelSiteRental(userFromCookie, context);
                        break;
                    case "getPriceConfigDT":
                        PriceConfigBLL priceConfigBll = new PriceConfigBLL();
                        context.Response.Write(priceConfigBll.GetPriceConfigDT(1));
                        break;
                    case "setTotalPrice":
                        string priceConfigID = context.Request.Params["priceConfigID"] ?? "";
                        string longer = context.Request.Params["longer"] ?? "";
                        if (priceConfigID != string.Empty)
                        {
                            PriceConfigBLL configBll = new PriceConfigBLL();
                            Model.PriceConfigModel model = configBll.GetPriceConfigModel(Int32.Parse(priceConfigID));
                            try
                            {
                                decimal aaa = model.Price.Value * decimal.Parse(longer);
                                context.Response.Write("{\"totalPrice\":" + model.Price.Value * decimal.Parse(longer) + "}");
                            }
                            catch (System.Exception ex)
                            {
                                context.Response.Write("{\"totalPrice\":\"0\"}");
                            }
                        }
                        else
                        {
                            context.Response.Write("{\"totalPrice\":\"0\"}");
                        }
                        break;
                    default:
                        context.Response.Write("{\"msg\":\"参数错误！\",\"success\":false}");
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"msg\":\"" + DriveMgr.Common.JsonHelper.StringFilter(ex.Message) + "\",\"success\":false}");
                userOperateLog.OperateInfo = "用户功能异常";
                userOperateLog.IfSuccess = false;
                userOperateLog.Description = DriveMgr.Common.JsonHelper.StringFilter(ex.Message);
                DriveMgr.BLL.UserOperateLog.InsertOperateInfo(userOperateLog);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 添加场地出租信息
        /// </summary>
        /// <param name="userFromCookie"></param>
        /// <param name="context"></param>
        private void AddSiteRental(DriveMgr.Model.User userFromCookie, HttpContext context)
        {
            if (userFromCookie != null && new DriveMgr.BLL.Authority().IfAuthority("siteRental_local", "pay", userFromCookie.Id))
            {
                int payid = Int32.Parse(context.Request.Params["payid"] ?? "-1");
                string ui_siteRental_RentObject_add = context.Request.Params["ui_siteRental_RentObject_pay"] ?? "";
                string ui_siteRental_RentDate_add = context.Request.Params["ui_siteRental_RentDate_pay"] ?? "";
                string ui_siteRental_LicencePlateNum_add = context.Request.Params["ui_siteRental_LicencePlateNum_pay"] ?? "";
                string ui_siteRental_PriceConfig_add = context.Request.Params["ui_siteRental_PriceConfig_pay"] ?? "";
                string ui_siteRental_Longer_add = context.Request.Params["ui_siteRental_Longer_pay"] ?? "";
                string ui_siteRental_TotalPrice_add = context.Request.Params["ui_siteRental_TotalPrice_pay"] ?? "";
                string ui_siteRental_Remark_add = context.Request.Params["ui_siteRental_Remark_pay"] ?? "";

                DriveMgr.Model.SiteRentalLocalModel siteRentalAdd = new Model.SiteRentalLocalModel();
                siteRentalAdd.StudentsID = payid;  //学员ID
                if (string.IsNullOrEmpty(ui_siteRental_PriceConfig_add.Trim()))
                {
                    siteRentalAdd.VehicleId = null;
                }
                else
                {
                    siteRentalAdd.VehicleId = Int32.Parse(ui_siteRental_LicencePlateNum_add.Trim());
                }
                siteRentalAdd.PriceConfigID = Int32.Parse(ui_siteRental_PriceConfig_add.Trim());
                siteRentalAdd.Longer = decimal.Parse(ui_siteRental_Longer_add.Trim());
                siteRentalAdd.TotalPrice = decimal.Parse(ui_siteRental_TotalPrice_add.Trim());
                siteRentalAdd.RentDate = DateTime.Parse(ui_siteRental_RentDate_add.Trim());
                siteRentalAdd.Remark = ui_siteRental_Remark_add.Trim();

                siteRentalAdd.CreateDate = DateTime.Now;
                siteRentalAdd.CreatePerson = userFromCookie.UserId;
                siteRentalAdd.UpdatePerson = userFromCookie.UserId;
                siteRentalAdd.UpdateDate = DateTime.Now;

                if (siteRentalBll.AddSiteRental(siteRentalAdd))
                {
                    userOperateLog.OperateInfo = payid+"添加场地出租信息";
                    userOperateLog.IfSuccess = true;
                    userOperateLog.Description = "添加成功，场地出租信息：" + ui_siteRental_LicencePlateNum_add.Trim();
                    context.Response.Write("{\"msg\":\"添加成功！\",\"success\":true}");
                }
                else
                {
                    userOperateLog.OperateInfo = "添加场地出租信息";
                    userOperateLog.IfSuccess = false;
                    userOperateLog.Description = "添加失败";
                    context.Response.Write("{\"msg\":\"添加失败！\",\"success\":false}");
                }
            }
            else
            {
                userOperateLog.OperateInfo = "添加场地出租信息";
                userOperateLog.IfSuccess = false;
                userOperateLog.Description = "无权限，请联系管理员";
                context.Response.Write("{\"msg\":\"无权限，请联系管理员！\",\"success\":false}");
            }
            DriveMgr.BLL.UserOperateLog.InsertOperateInfo(userOperateLog);
        }

        /// <summary>
        /// 编辑车辆
        /// </summary>
        /// <param name="userFromCookie"></param>
        /// <param name="context"></param>
        private void EditSiteRental(DriveMgr.Model.User userFromCookie, HttpContext context)
        {
            if (userFromCookie != null && new DriveMgr.BLL.Authority().IfAuthority("siteRental_local", "edit", userFromCookie.Id))
            {
                int id = Convert.ToInt32(context.Request.Params["id"]);
                DriveMgr.Model.SiteRentalLocalModel siteRentalEdit = siteRentalBll.GetSiteRentalModel(id);
                string ui_siteRental_RentObject_edit = context.Request.Params["ui_siteRental_RentObject_payedit"] ?? "";
                string ui_siteRental_RentDate_edit = context.Request.Params["ui_siteRental_RentDate_payedit"] ?? "";
                string ui_siteRental_LicencePlateNum_edit = context.Request.Params["ui_siteRental_LicencePlateNum_payedit"] ?? "";
                string ui_siteRental_PriceConfig_edit = context.Request.Params["ui_siteRental_PriceConfig_payedit"] ?? "";
                string ui_siteRental_Longer_edit = context.Request.Params["ui_siteRental_Longer_payedit"] ?? "";
                string ui_siteRental_TotalPrice_edit = context.Request.Params["ui_siteRental_TotalPrice_payedit"] ?? "";
                string ui_siteRental_Remark_edit = context.Request.Params["ui_siteRental_Remark_payedit"] ?? "";

                //siteRentalEdit.RentObject = ui_siteRental_RentObject_edit.Trim();
                if (string.IsNullOrEmpty(ui_siteRental_LicencePlateNum_edit.Trim()))
                {
                    siteRentalEdit.VehicleId = null;
                }
                else
                {
                    siteRentalEdit.VehicleId = Int32.Parse(ui_siteRental_LicencePlateNum_edit.Trim());
                }
                siteRentalEdit.PriceConfigID = Int32.Parse(ui_siteRental_PriceConfig_edit.Trim());
                siteRentalEdit.Longer = decimal.Parse(ui_siteRental_Longer_edit.Trim());
                siteRentalEdit.TotalPrice = decimal.Parse(ui_siteRental_TotalPrice_edit.Trim());
                siteRentalEdit.RentDate = DateTime.Parse(ui_siteRental_RentDate_edit.Trim());
                siteRentalEdit.Remark = ui_siteRental_Remark_edit.Trim();

                siteRentalEdit.UpdatePerson = userFromCookie.UserId;
                siteRentalEdit.UpdateDate = DateTime.Now;

                if (siteRentalBll.UpdateSiteRental(siteRentalEdit))
                {
                    userOperateLog.OperateInfo = "修改场地出租信息";
                    userOperateLog.IfSuccess = true;
                    userOperateLog.Description = "修改成功，场地出租信息主键：" + siteRentalEdit.Id;
                    context.Response.Write("{\"msg\":\"修改成功！\",\"success\":true}");
                }
                else
                {
                    userOperateLog.OperateInfo = "修改场地出租信息";
                    userOperateLog.IfSuccess = false;
                    userOperateLog.Description = "修改失败";
                    context.Response.Write("{\"msg\":\"修改失败！\",\"success\":false}");
                }
            }
            else
            {
                userOperateLog.OperateInfo = "修改场地出租信息";
                userOperateLog.IfSuccess = false;
                userOperateLog.Description = "无权限，请联系管理员";
                context.Response.Write("{\"msg\":\"无权限，请联系管理员！\",\"success\":false}");
            }
            DriveMgr.BLL.UserOperateLog.InsertOperateInfo(userOperateLog);
        }

        /// <summary>
        /// 删除车辆
        /// </summary>
        /// <param name="userFromCookie"></param>
        /// <param name="context"></param>
        private void DelSiteRental(DriveMgr.Model.User userFromCookie, HttpContext context)
        {
            if (userFromCookie != null && new DriveMgr.BLL.Authority().IfAuthority("siteRental", "delete", userFromCookie.Id))
            {
                string ids = context.Request.Params["id"].Trim(',');
                if (siteRentalBll.DeleteSiteRentalList(ids))
                {
                    userOperateLog.OperateInfo = "删除场地出租信息";
                    userOperateLog.IfSuccess = true;
                    userOperateLog.Description = "删除成功，场地出租信息主键：" + ids;
                    context.Response.Write("{\"msg\":\"删除成功！\",\"success\":true}");
                }
                else
                {
                    userOperateLog.OperateInfo = "删除场地出租信息";
                    userOperateLog.IfSuccess = false;
                    userOperateLog.Description = "删除失败";
                    context.Response.Write("{\"msg\":\"删除失败！\",\"success\":false}");
                }
            }
            else
            {
                userOperateLog.OperateInfo = "删除场地出租信息";
                userOperateLog.IfSuccess = false;
                userOperateLog.Description = "无权限，请联系管理员";
                context.Response.Write("{\"msg\":\"无权限，请联系管理员！\",\"success\":false}");
            }
            DriveMgr.BLL.UserOperateLog.InsertOperateInfo(userOperateLog);
        }
    }
}