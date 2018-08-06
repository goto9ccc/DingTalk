﻿using DingTalk.Models;
using DingTalk.Models.DingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DingTalk.Controllers
{
    [RoutePrefix("VoteManager")]
    /// <summary>
    /// 投票管理
    /// </summary>
    public class VoteManagerController : ApiController
    {
        /// <summary>
        /// 发起投票
        /// </summary>
        /// <param name="vote"></param>
        [Route("LaunchVote")]
        [HttpPost]
        public object LaunchVote([FromBody] Vote vote)
        {
            try
            {
                using (DDContext context =new DDContext ())
                {
                    context.Vote.Add(vote);
                    context.SaveChanges();
                }
                return new ErrorModel()
                {
                    errorCode=0,
                    errorMessage="成功发起"
                };
            }
            catch (Exception ex )
            {
                return new ErrorModel()
                {
                    errorCode = 1,
                    errorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 修改投票
        /// </summary>
        /// <param name="vote"></param>
        [Route("ChangeVote")]
        [HttpPost]
        public object ChangeVote([FromBody] Vote vote)
        {
            try
            {
                using (DDContext context = new DDContext())
                {
                    context.Entry<Vote>(vote).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return new ErrorModel()
                {
                    errorCode = 0,
                    errorMessage = "修改投票成功"
                };
            }
            catch (Exception ex)
            {
                return new ErrorModel()
                {
                    errorCode = 1,
                    errorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 读取投票信息
        /// </summary>
        /// <param name="Id"></param>
        public object QuaryVote(string Id)
        {
            try
            {
                using (DDContext context = new DDContext())
                {
                    Vote vote= context.Vote.Find(Id);
                    return vote;
                }
            }
            catch (Exception ex)
            {
                return new ErrorModel()
                {
                    errorCode = 1,
                    errorMessage = ex.Message
                };
            }
        }
    }
}
