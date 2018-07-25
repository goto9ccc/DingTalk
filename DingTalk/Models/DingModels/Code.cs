namespace DingTalk.Models.DingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Code")]
    public partial class Code
    {
        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        /// <summary>
        /// ��ˮ��
        /// </summary>
        [StringLength(30)]
        public string TaskId { get; set; }
        /// <summary>
        /// ���ϱ���
        /// </summary>
        [StringLength(500)]
        public string CodeNumber { get; set; }
        /// <summary>
        /// ���ϴ������
        /// </summary>
        [StringLength(500)]
        public string BigCode { get; set; }
        /// <summary>
        /// ����С�����
        /// </summary>
        [StringLength(500)]
        public string SmallCode { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        [StringLength(500)]
        public string Name { get; set; }
        /// <summary>
        /// ��λ
        /// </summary>
        [StringLength(500)]
        public string Unit { get; set; }
        /// <summary>
        /// ����ͺ�
        /// </summary>
        [StringLength(500)]
        public string Standard { get; set; }
        /// <summary>
        /// ���洦��
        /// </summary>
        [StringLength(500)]
        public string SurfaceTreatment { get; set; }
        /// <summary>
        /// ���ܵȼ�
        /// </summary>
        [StringLength(500)]
        public string PerformanceLevel { get; set; }
        /// <summary>
        /// ��׼��
        /// </summary>
        [StringLength(500)]
        public string StandardNumber { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        [StringLength(500)]
        public string Features { get; set; }
        /// <summary>
        /// ��;
        /// </summary>
        [StringLength(500)]
        public string purpose { get; set; }
        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(500)]
        public string Remark { get; set; }
    }
}
