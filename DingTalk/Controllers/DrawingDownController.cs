﻿using Common.JsonHelper;
using DingTalk.Bussiness.FlowInfo;
using DingTalk.Models;
using DingTalk.Models.DbModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DingTalk.Controllers
{
    public class DrawingDownController : Controller
    {
        // GET: DrawingDown
        public ActionResult Index()
        {
            return View();
        }

        #region 图纸上传数据读取
        /// <summary>
        /// 图纸上传数据读取(图纸上传流程已结束)
        /// </summary>
        /// <param name="ProjectId">项目Id</param>
        /// <returns></returns>
        /// 测试数据：/DrawingDown/GetDrawingDownInfo?ProjectId=2016ZL051&ApplyManId=123456
        [HttpGet]
        public string GetDrawingDownInfo(string ProjectId, string ApplyManId)
        {
            try
            {
                using (DDContext context = new DDContext())
                {
                    string CheckApplyManId = context.NodeInfo.Where(u => u.FlowId == "7" && u.NodeId == 0).First().PeopleId;
                    if (ApplyManId != CheckApplyManId)
                    {
                        return JsonConvert.SerializeObject(new ErrorModel
                        {
                            errorCode = 1,
                            errorMessage = "没有权限发起次流程"
                        });
                    }
                    else
                    {
                        List<Tasks> TaskIdList = FlowInfoServer.ReturnUnFinishedTaskId("6");
                        var TaskIdLists = from t in TaskIdList
                                          where
                     t.ProjectId == ProjectId
                                          select t;
                        //获取并过滤已完成流程的TaskId
                        List<string> ListTaskId = new List<string>();
                        foreach (var item in TaskIdLists)
                        {
                            if (!ListTaskId.Contains(item.TaskId.ToString()))
                            {
                                ListTaskId.Add(item.TaskId.ToString());
                            }
                        }

                        var TaskList = from t in ListTaskId select t;

                        var Purchase = context.Purchase.Where(u => u.IsDown != true);
                        var Quary = from t in TaskList
                                    join p in Purchase
                                    on t.ToString() equals p.TaskId
                                    select new
                                    {
                                        p.Id,
                                        p.DrawingNo,
                                        p.Name,
                                        p.Sorts,
                                        p.TaskId,
                                        p.MaterialScience,
                                        p.Brand,
                                        p.Count,
                                        p.Unit,
                                        p.Mark,
                                    };
                        return JsonConvert.SerializeObject(Quary);
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }
        #endregion

        //#region 已上传的BOM表查询

        ///// <summary>
        ///// 已上传的BOM表查询
        ///// </summary>
        ///// <param name="ApplyManId"></param>
        ///// <returns></returns>
        ///// /DrawingDown/GetPurchaseInfo?ApplyManId=123456
        //[HttpGet]
        //public string GetPurchaseInfo(string ApplyManId)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(ApplyManId))
        //        {
        //            return JsonConvert.SerializeObject(new ErrorModel
        //            {
        //                errorCode = 1,
        //                errorMessage = "未传递参数ApplyManId"
        //            });
        //        }
        //        else
        //        {
        //            using (DDContext context = new DDContext())
        //            {

        //                string PeopleId = context.NodeInfo.Where(u => u.NodeId == 0 && u.FlowId == "7").Select(u => u.PeopleId).First();
        //                if (ApplyManId != PeopleId) //校对申请人
        //                {
        //                    return JsonConvert.SerializeObject(new ErrorModel
        //                    {
        //                        errorCode = 3,
        //                        errorMessage = "用户没有权限"
        //                    });
        //                }
        //                else
        //                {
        //                    List<Purchase> PurchaseList = context.Purchase.Where(u => u.IsDown == false).ToList();
        //                    return JsonConvert.SerializeObject(PurchaseList);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return JsonConvert.SerializeObject(new ErrorModel
        //        {
        //            errorCode = 2,
        //            errorMessage = ex.Message
        //        });
        //    }
        //}

        //#endregion

        #region 图纸下发表单提交

        /// <summary>
        /// 图纸下发表单提交接口
        /// </summary>
        /// <returns></returns>
        /// 测试数据  /DrawingDown/SubmitDrawingDown
        /// var PurchaseDownList =  [{ "DrawingNo": "DTE-801B-WX-01C", "OldTaskId": "中料", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"},
        ///  { "DrawingNo": "DTE-801B-WX-01C", "ProcedureName": "喷漆", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"},
        ///  { "DrawingNo": "DTE-801B-WX-01D", "ProcedureName": "切割", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"}] 
        [HttpPost]
        public string SubmitDrawingDown(List<PurchaseDown> PurchaseDownList)
        {
            try
            {
                if (PurchaseDownList == null)
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "请传递参数"
                    });
                }
                else
                {
                    using (DDContext context = new DDContext())
                    {
                        foreach (PurchaseDown purchaseDown in PurchaseDownList)
                        {
                            purchaseDown.IsDown = true;
                            context.PurchaseDown.Add(purchaseDown);
                        }
                        context.SaveChanges();
                    }
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 0,
                        errorMessage = "保存成功"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }

        #endregion

        #region 添加工序与工时

        /// <summary>
        /// 添加工序
        /// </summary>
        /// <returns></returns>
        /// 测试数据: /DrawingDown/AddProcedure
        ///  var PurchaseList = [{ "DrawingNo": "DTE-801B-WX-01C", "ProcedureName": "中料", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"},
        ///  { "DrawingNo": "DTE-801B-WX-01C", "ProcedureName": "喷漆", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"},
        ///  { "DrawingNo": "DTE-801B-WX-01D", "ProcedureName": "切割", "DefaultWorkTime": "1", "State": "0", "CreateTime": "2018-04-24 15:48", "ApplyMan": "胡工", "ApplyManId": "123456"}] 
        [HttpPost]
        public string AddProcedure()
        {
            try
            {
                StreamReader reader = new StreamReader(Request.InputStream);
                string List = reader.ReadToEnd();
                if (string.IsNullOrEmpty(List))
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "请传递参数"
                    });
                }
                else
                {
                    List<ProcedureInfo> procedureInfoList = new List<ProcedureInfo>();
                    procedureInfoList = JsonHelper.JsonToObject<List<ProcedureInfo>>(List);
                    using (DDContext context = new DDContext())
                    {
                        foreach (ProcedureInfo procedureInfo in procedureInfoList)
                        {
                            context.ProcedureInfo.Add(procedureInfo);
                        }
                        context.SaveChanges();
                    }
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 0,
                        errorMessage = "保存成功"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// 添加工时
        /// </summary>
        /// <returns></returns>
        /// 测试数据: /DrawingDown/AddWorkTime
        ///  var WorkTimeList = [{ "ProjectInfoId": "1", "IsFinish": false, "Worker": "小红", "WorkerId": "666", "StartTime": "2018-04-24 15:48", "EndTime": "2018-04-25 15:48", "UseTime": "2"},
        ///  { "ProjectInfoId": "2", "IsFinish": false, "Worker": "小滨", "WorkerId": "777", "StartTime": "2018-04-24 15:48", "EndTime": "2018-04-25 15:48", "UseTime": "3"},
        ///  { "ProjectInfoId": "2", "IsFinish": true, "Worker": "小雨", "WorkerId": "888", "StartTime": "2018-04-24 15:48", "EndTime": "2018-04-25 15:48", "UseTime": "3"}] 
        [HttpPost]
        public string AddWorkTime()
        {
            try
            {
                StreamReader reader = new StreamReader(Request.InputStream);
                string List = reader.ReadToEnd();
                if (string.IsNullOrEmpty(List))
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "请传递参数"
                    });
                }
                else
                {
                    List<WorkTime> WorkTimeInfoList = new List<WorkTime>();
                    WorkTimeInfoList = JsonHelper.JsonToObject<List<WorkTime>>(List);
                    using (DDContext context = new DDContext())
                    {
                        foreach (WorkTime workTime in WorkTimeInfoList)
                        {
                            context.WorkTime.Add(workTime);
                        }
                        context.SaveChanges();
                    }
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 0,
                        errorMessage = "保存成功"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }

        #endregion

        #region Bom表、工序、工时数据读取

        /// <summary>
        /// Bom表、工序、工时数据读取
        /// </summary>
        /// <param name="TaskId">流水号</param>
        /// <returns></returns>
        /// 测试数据: /DrawingDown/GetAllInfo?TaskId=2
        [HttpGet]
        public string GetAllInfo(int TaskId = 0)
        {
            try
            {
                if (TaskId == 0)
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "请传递参数"
                    });
                }
                else
                {
                    using (DDContext context = new DDContext())
                    {
                        List<Purchase> PurchaseList = context.Purchase.
                            Where(u => u.TaskId == TaskId.ToString()).ToList();
                        List<ProcedureInfo> ProcedureInfoList = context.ProcedureInfo.ToList();
                        List<WorkTime> WorkTimeInfoList = context.WorkTime.ToList();
                        var Quary = from p in PurchaseList
                                    join s in ProcedureInfoList
                                    on p.DrawingNo equals s.DrawingNo
                                    join w in WorkTimeInfoList
                                    on s.Id.ToString() equals w.ProjectInfoId
                                    select new
                                    {
                                        p.TaskId,
                                        p.IsDown,
                                        p.Mark,
                                        p.MaterialScience,
                                        p.Name,
                                        p.Sorts,
                                        s.ApplyMan,
                                        s.ApplyManId,
                                        s.CreateTime,
                                        s.DefaultWorkTime,
                                        s.DrawingNo,
                                        w.IsFinish,
                                        w.ProjectInfoId,
                                        w.StartTime,
                                        w.EndTime,
                                        w.UseTime,
                                        w.Worker,
                                        w.WorkerId
                                    };
                        return JsonConvert.SerializeObject(Quary);
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }

        #endregion

        #region 工序信息查询

        /// <summary>
        /// 工序信息查询
        /// </summary>
        /// <param name="DrawingNo">零件编号</param>
        /// <returns></returns>
        /// 测试数据：/DrawingDown/GetProcedureInfo?DrawingNo=DTE-801B-WX-01C
        [HttpGet]
        public string GetProcedureInfo(string DrawingNo)
        {
            try
            {
                if (string.IsNullOrEmpty(DrawingNo))
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "请传递参数DrawingNo"
                    });
                }
                else
                {
                    using (DDContext context = new DDContext())
                    {
                        List<ProcedureInfo> ListProcedureInfo = context.ProcedureInfo.Where(u => u.DrawingNo == DrawingNo).ToList();
                        return JsonConvert.SerializeObject(ListProcedureInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 2,
                    errorMessage = ex.Message
                });
            }
        }

        #endregion

        #region 员工信息查询

        /// <summary>
        /// 员工信息查询
        /// </summary>
        /// <returns></returns>
        /// 测试数据：测试数据：/DrawingDown/GetWorkerInfo
        public string GetWorkerInfo()
        {
            try
            {
                using (DDContext context = new DDContext())
                {
                    List<Worker> ListWorker = context.Worker.ToList();
                    return JsonConvert.SerializeObject(ListWorker);
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 1,
                    errorMessage = ex.Message
                });
            }
        }

        #endregion
    }
}