﻿using Common.ClassChange;
using Common.DTChange;
using Common.Excel;
using Common.Ionic;
using Common.JsonHelper;
using Common.PDF;
using DingTalk.Models;
using DingTalk.Models.DingModels;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DingTalk.Controllers
{
    public class DrawingUploadController : Controller
    {
        // GET: DrawingUpload
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 文件上传接口
        /// </summary>
        /// <param name="form"></param>
        /// <param name="path">文件路径</param>
        /// <param name="ApplyMan">用户Id</param>
        /// <param name="ApplyManId">用户Id</param>
        /// <returns>返回文件路径</returns>
        [HttpPost]
        public async Task<string> Upload(FormCollection form, string path, string ApplyMan, string ApplyManId)
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    //Request.Files.Count 文件数为0上传不成功
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "文件数为0"
                    });
                }

                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    //文件大小大（以字节为单位）为0时，做一些操作
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 2,
                        errorMessage = "文件大小大（以字节为单位）为0"
                    });
                }
                else
                {
                    //文件大小不为0
                    HttpPostedFileBase files = Request.Files[0];
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string Path = "";
                    string strPath = "";
                    string FileName = file.FileName;
                    string strExtension = "";
                    int nIndex = FileName.LastIndexOf('.');
                    if (nIndex >= 0)
                    {
                        strExtension = FileName.Substring(nIndex);
                    }

                    if (string.IsNullOrEmpty(path))
                    {
                        switch (strExtension)
                        {
                            //Image
                            case ".jpg":
                                strPath = @"~\UploadFile\Images\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                            case ".png":
                                strPath = @"~\UploadFile\Images\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                            //Excel
                            case ".xls":
                                strPath = @"~\UploadFile\Excel\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                            case ".xlsx":
                                strPath = @"~\UploadFile\Excel\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                            case ".pdf":
                                strPath = @"~\UploadFile\PDF\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                            //其他文件
                            default:
                                strPath = @"~\UploadFile\Flies\";
                                Path = Server.MapPath(strPath + newFileName + strExtension);
                                break;
                        }

                        //保存文件
                        files.SaveAs(Path);
                    }
                    else
                    {
                        Path = Server.MapPath(path + "\\" + FileName);
                        FileInfos fileInfos = new FileInfos()
                        {
                            ApplyMan = ApplyMan,
                            ApplyManId = ApplyManId,
                            FilePath = path + "\\" + FileName,
                            LastModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:hh:ss"),
                            LastModifyState = "0"
                        };

                        using (DDContext context = new DDContext())
                        {
                            int j = GetIndexOfString(path, "\\", 6);
                            string CheckPath = path.Substring(0, j - 1);
                            bool IsComPower = (context.ProjectInfo.Where(p => p.ResponsibleManId == ApplyManId && p.FilePath == CheckPath).ToList().Count() >= 1) ? true : false;
                            //判断权限
                            bool IsSuperPower = (context.Roles.Where(r => r.UserId == ApplyManId && r.RoleName == "超级管理员").ToList().Count() >= 1) ? true : false;
                            if (IsComPower || IsSuperPower)
                            {
                                //保存文件
                                files.SaveAs(Path);

                                //上传盯盘获取MediaId
                                var otherController = DependencyResolver.Current.GetService<DingTalkServersController>();
                                var resultUploadMedia = await otherController.UploadMedia(fileInfos.FilePath);
                                FileSendModel fileSendModel = JsonConvert.DeserializeObject<FileSendModel>(resultUploadMedia);
                                fileInfos.MediaId = fileSendModel.Media_Id;
                                context.FileInfos.Add(fileInfos);
                                context.SaveChanges();
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new ErrorModel
                                {
                                    errorCode = 1,
                                    errorMessage = "用户没有权限进行操作"
                                });
                            }
                        }
                    }

                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 0,
                        errorMessage = "上传成功",
                        Content = strPath + newFileName + strExtension
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 3,
                    errorMessage = ex.Message
                });
            }

        }


        /// <summary>
        /// 上传Excel并读取数据接口
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAndGetInfo(FormCollection form)
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    //Request.Files.Count 文件数为0上传不成功
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "文件数为0"
                    });
                }

                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    //文件大小大（以字节为单位）为0时，做一些操作
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 2,
                        errorMessage = "文件大小大（以字节为单位）为0"
                    });
                }
                else
                {
                    //文件大小不为0
                    HttpPostedFileBase files = Request.Files[0];
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                    string Path = Server.MapPath(@"~\UploadFile\Excel\" + newFileName);
                    files.SaveAs(Path);
                    return LoadExcel(Path);
                    //    JsonConvert.SerializeObject(new ErrorModel
                    //{
                    //    errorCode = 0,
                    //    errorMessage = "上传成功",
                    //    Content = Server.MapPath(@"~\UploadFile\Excel\" + newFileName)
                    //});
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 3,
                    errorMessage = ex.Message
                });
            }

        }

        /// <summary>
        /// Excel读取
        /// </summary>
        /// <param name="Path">Excel路径</param>
        /// <returns></returns>
        /// 测试数据/DrawingUpload/LoadExcel?path=C:\\Users\\tong\\Source\\Repos\\DingTalk\\DingTalk\\UploadFile\\Excel\\20180408163044.xlsx
        [HttpGet]
        public string LoadExcel(string Path)
        {
            if (Path == null)  //测试暂用
            {
                Path = "C:\\Users\\tong\\Source\\Repos\\DingTalk\\DingTalk\\UploadFile\\Excel\\BOM表提交模板.xls";
            }

            DataTable db = ExcelHelperByNPOI.ImportExcel2003toDt(Path);
            Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
            dic.Add(Path, db);
            return JsonConvert.SerializeObject(dic.ToArray());
        }



        /// <summary>
        /// BOM表信息查询
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        /// 测试数据：/DrawingUpload/GetPurchase?TaskId=2
        [HttpGet]
        public string GetPurchase(string TaskId)
        {
            try
            {
                if (string.IsNullOrEmpty(TaskId))
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "TaskId不能为空！"
                    });
                }
                else
                {
                    using (DDContext context = new DDContext())
                    {
                        var PurchaseList = context.Purchase.Where(u => u.TaskId == TaskId);
                        if (PurchaseList != null)
                        {
                            return JsonConvert.SerializeObject(PurchaseList);
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new ErrorModel
                            {
                                errorCode = 2,
                                errorMessage = "未查到数据！"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 3,
                    errorMessage = ex.Message
                });
            }
        }


        /// <summary>
        /// BOM表信息载入
        /// </summary>
        /// <returns></returns>
        /// 测试数据 /DrawingUpload/LoadPurchase
        ///  var PurchaseList = [{ "TaskId": "2","BomId":"1", "DrawingNo": "BOM-2017QZL001-delta0505A机器人", "CodeNo": "1", "Name": "十字座D16", "Count": "1", "MaterialScience": "7075T6", "Unit": "件", "Brand": "ATMV", "Sorts": "自制", "Mark": "借用" },
        ///  { "TaskId": "2", "BomId":"2","DrawingNo": "DTE-801B-WX-01C", "CodeNo": "2", "Name": "十字座套", "Count": "2", "MaterialScience": "7075T6", "Unit": "件", "Brand": "耐克", "Sorts": "自制", "Mark": "借用" },
        ///  { "TaskId": "2","BomId":"3", "DrawingNo": "DTE-801B-WX-01B", "CodeNo": "3", "Name": "十字座D10", "Count": "1", "MaterialScience": "7075T6", "Unit": "件", "Brand": "阿迪", "Sorts": "自制", "Mark": "借用" }]
        [HttpPost]
        public string LoadPurchase()
        {
            try
            {
                StreamReader reader = new StreamReader(Request.InputStream);
                string PurchaseJson = reader.ReadToEnd();
                if (string.IsNullOrEmpty(PurchaseJson))
                {
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 1,
                        errorMessage = "载入数据不能为空！"
                    });
                }
                else
                {
                    List<Purchase> listPurchase = new List<Purchase>();
                    listPurchase = JsonHelper.JsonToObject<List<Purchase>>(PurchaseJson);
                    foreach (Purchase item in listPurchase)
                    {
                        using (DDContext context = new DDContext())
                        {
                            context.Purchase.Add(item);
                            context.SaveChanges();
                        }
                    }
                    return JsonConvert.SerializeObject(new ErrorModel
                    {
                        errorCode = 0,
                        errorMessage = "载入成功!"
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
        /// 打印表单数据、盖章、推送
        /// </summary>
        /// 测试数据   /DrawingUpload/PrintAndSend
        /// data: { "UserId":"083452125733424957","TaskId":"3","OldPath":"~\\UploadFile\\Flies\\20180621103648.PDF,~\\UploadFile\\Flies\\20180621103649.PDF" }
        [HttpPost]
        public async Task<string> PrintAndSend(PrintAndSendModel printAndSendModel)
        {
            try
            {
                string TaskId = printAndSendModel.TaskId;
                string UserId = printAndSendModel.UserId;
                string OldPath = printAndSendModel.OldPath;
                PDFHelper pdfHelper = new PDFHelper();
                using (DDContext context = new DDContext())
                {
                    //获取表单信息
                    Tasks tasks = context.Tasks.Where(t => t.TaskId.ToString() == TaskId && t.NodeId == 0).First();
                    string FlowId = tasks.FlowId.ToString();
                    string ProjectId = tasks.ProjectId;

                    ////判断是否有权限触发按钮
                    //string PeopleId = context.NodeInfo.Where(n => n.NodeName == "行政盖章" && n.FlowId == FlowId).First().PeopleId;
                    //if (UserId != PeopleId)
                    //{
                    //    return JsonConvert.SerializeObject(new ErrorModel
                    //    {
                    //        errorCode = 1,
                    //        errorMessage = "没有权限"
                    //    });
                    //}
                    //判断流程是否已结束
                    List<Tasks> tasksList = context.Tasks.Where(t => t.TaskId.ToString() == TaskId && t.IsSend != true && t.State == 0).ToList();
                    if (tasksList.Count > 0)
                    {
                        return JsonConvert.SerializeObject(new ErrorModel
                        {
                            errorCode = 2,
                            errorMessage = "流程未结束"
                        });
                    }

                    List<Purchase> PurchaseList = context.Purchase.Where(u => u.TaskId == TaskId).ToList();

                    var SelectPurchaseList = from p in PurchaseList
                                             select new
                                             {
                                                 p.DrawingNo,
                                                 p.Name,
                                                 p.Count,
                                                 p.MaterialScience,
                                                 p.Unit,
                                                 p.SingleWeight,
                                                 p.AllWeight,
                                                 p.Sorts,
                                                 p.NeedTime,
                                                 p.Mark
                                             };

                    DataTable dtSourse = DtLinqOperators.CopyToDataTable(SelectPurchaseList);
                    //ClassChangeHelper.ToDataTable(SelectPurchaseList);
                    List<NodeInfo> NodeInfoList = context.NodeInfo.Where(u => u.FlowId == FlowId && u.NodeId != 0 && u.NodeName != "结束").ToList();
                    foreach (NodeInfo nodeInfo in NodeInfoList)
                    {
                        if (string.IsNullOrEmpty(nodeInfo.NodePeople))
                        {
                            string strNodePeople = context.Tasks.Where(q => q.TaskId.ToString() == TaskId && q.NodeId == nodeInfo.NodeId).First().ApplyMan;
                            string ApplyTime = context.Tasks.Where(q => q.TaskId.ToString() == TaskId && q.NodeId == nodeInfo.NodeId).First().ApplyTime;
                            nodeInfo.NodePeople = strNodePeople + "  " + ApplyTime;
                        }
                        else
                        {
                            string ApplyTime = context.Tasks.Where(q => q.TaskId.ToString() == TaskId && q.NodeId == nodeInfo.NodeId).First().ApplyTime;
                            nodeInfo.NodePeople = nodeInfo.NodePeople + "  " + ApplyTime;
                        }
                    }
                    DataTable dtApproveView = ClassChangeHelper.ToDataTable(NodeInfoList);
                    string FlowName = context.Flows.Where(f => f.FlowId.ToString() == FlowId).First().FlowName.ToString();
                    string ProjectName = context.ProjectInfo.Where(p => p.ProjectId == ProjectId).First().ProjectName;
                    //绘制BOM表单PDF
                    List<string> contentList = new List<string>()
                        {
                            "序号","代号","名称","数量","材料","单位","单重","总重","类别","需用日期","备注"
                        };

                    float[] contentWithList = new float[]
                    {
                        50, 60, 60, 30, 60, 60, 60, 60, 60 , 60, 60
                    };

                    string path = pdfHelper.GeneratePDF(FlowName, TaskId, tasks.ApplyMan, tasks.ApplyTime,
                    ProjectName, "1", 380, 710, contentList, contentWithList, dtSourse, dtApproveView);
                    string RelativePath = "~/UploadFile/PDF/" + Path.GetFileName(path);

                    string[] Paths = OldPath.Split(',');

                    List<string> newPaths = new List<string>();
                    RelativePath = AppDomain.CurrentDomain.BaseDirectory + RelativePath.Substring(2, RelativePath.Length - 2).Replace('/', '\\');
                    newPaths.Add(RelativePath);
                    foreach (string pathChild in Paths)
                    {
                        string AbPath = AppDomain.CurrentDomain.BaseDirectory + pathChild.Substring(2, pathChild.Length - 2);
                        //PDF盖章 保存路径
                        newPaths.Add(pdfHelper.PDFWatermark(AbPath,
                        string.Format(@"{0}\UploadFile\PDF\{1}",
                        AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(pathChild)),
                        string.Format(@"{0}\Content\images\受控章.png", AppDomain.CurrentDomain.BaseDirectory),
                        100, 100));
                    }
                    string SavePath = string.Format(@"{0}\UploadFile\Ionic\{1}.zip", AppDomain.CurrentDomain.BaseDirectory, "图纸审核" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    //文件压缩打包
                    IonicHelper.CompressMulti(newPaths, SavePath, false);

                    //上传盯盘获取MediaId
                    var otherController = DependencyResolver.Current.GetService<DingTalkServersController>();
                    SavePath = string.Format(@"~\UploadFile\Ionic\{0}", Path.GetFileName(SavePath));
                    var resultUploadMedia = await otherController.UploadMedia(SavePath);
                    //推送用户
                    FileSendModel fileSendModel = JsonConvert.DeserializeObject<FileSendModel>(resultUploadMedia);
                    fileSendModel.UserId = UserId;
                    var result = await otherController.SendFileMessage(fileSendModel);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ErrorModel
                {
                    errorCode = 3,
                    errorMessage = ex.Message
                });
            }
        }

        public int GetIndexOfString(string InputString, string CharString, int n)
        {
            int count = 0;
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                int j = InputString.IndexOf(CharString);
                InputString = InputString.Substring(j + 1, InputString.Length - j - 1);
                count++;
                k = k + j + 1;
                if (count == n)
                {
                    return k;
                }
            }
            return 0;
        }

    }
}
