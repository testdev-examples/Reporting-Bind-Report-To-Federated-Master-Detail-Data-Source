using System;
using System.Windows.Forms;
using System.IO;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.DataFederation;
using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraReports.UI;

namespace BindReportToFederatedMasterDetailDataSource
{
    public partial class Form1 : Form
    {
        public Form1() {
            InitializeComponent();
        }
        void Button1_Click(object sender, EventArgs e) {
            ReportDesignTool designTool = new ReportDesignTool(CreateReport());
            designTool.ShowRibbonDesignerDialog();
        }
        static FederationDataSource CreateFederationDataSource(SqlDataSource sql, ExcelDataSource excel) {
            // Create SQL and Excel sources.
            Source sqlSource = new Source(sql.Name, sql, "Categories");
            Source excelSource = new Source(excel.Name, excel, "");

            // Create the federated "Categores" query and select data from the SQL source's query.
            var categoriesNode = sqlSource.From()
                .Select("CategoryID", "CategoryName", "Description").Build("Categories");
            // Create the federated "Products" query and select data from the Excel data source.
            var productsNode = excelSource.From()
                .Select("ProductName", "CategoryID", "UnitPrice").Build("Products");

            // Create a federated data source and add the federated queries to the collection.
            var federationDataSource = new FederationDataSource();
            federationDataSource.Queries.AddRange(new[] { categoriesNode, productsNode });
            // Specify a master-detail relationship between these queries based on the "CategoryID" key field.
            var relation = new FederationMasterDetailInfo("Categories", "Products", new FederationRelationColumnInfo("CategoryID", "CategoryID"));
            federationDataSource.Relations.Add(relation);
            // Build the data source schema to display it in the Field List.
            federationDataSource.RebuildResultSchema();

            return federationDataSource;
        }
        public static XtraReport CreateReport() {
            // Create a new report.
            var report = new XtraReport();

            // Add the Detail band to the master report and create a label bound to the "CategoryName" data field.
            var detailBand = new DetailBand() { HeightF = 25 };
            var categoryLabel = new XRLabel() { WidthF = 150 };
            categoryLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CategoryName]"));
            detailBand.Controls.Add(categoryLabel);

            // Create the detail report and add the Detail band to it.
            var detailReport = new DetailReportBand();
            var detailBand2 = new DetailBand() { HeightF = 25 };
            detailReport.Bands.Add(detailBand2);
            report.Bands.AddRange(new Band[] { detailBand, detailReport });
            // Add a label bound to the "ProductName" data field.
            var productLabel = new XRLabel() { WidthF = 300, LocationF = new PointF(100, 0) };
            productLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ProductName]"));
            detailBand2.Controls.Add(productLabel);

            // Create data sources. 
            var sqlDataSource = CreateSqlDataSource();
            var excelDataSource = CreateExcelDataSource();
            var federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource);
            // Add all data sources to the report to avoid serialization issues. 
            report.ComponentStorage.AddRange(new IComponent[] { sqlDataSource, excelDataSource, federationDataSource });
            // Assign a federated data source to the report and its detail report.
            report.DataSource = federationDataSource;
            report.DataMember = "Categories";
            detailReport.DataSource = federationDataSource;
            detailReport.DataMember = "Categories.CategoriesProducts";

            return report;
        }
        static SqlDataSource CreateSqlDataSource() {
            var connectionParameters = new Access97ConnectionParameters(Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "Data/nwind.mdb"), "", "");
            var sqlDataSource = new SqlDataSource(connectionParameters) { Name = "Sql_Categories" };
            var categoriesQuery = SelectQueryFluentBuilder.AddTable("Categories").SelectAllColumnsFromTable().Build("Categories");
            sqlDataSource.Queries.Add(categoriesQuery);
            sqlDataSource.RebuildResultSchema();
            return sqlDataSource;
        }
        static ExcelDataSource CreateExcelDataSource() {
            var excelDataSource = new ExcelDataSource() { Name = "Excel_Products" };
            excelDataSource.FileName = Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "Data/Products.xlsx");
            excelDataSource.SourceOptions = new ExcelSourceOptions() {
                ImportSettings = new ExcelWorksheetSettings("Sheet"),
            };
            excelDataSource.RebuildResultSchema();
            return excelDataSource;
        }
    }
}
