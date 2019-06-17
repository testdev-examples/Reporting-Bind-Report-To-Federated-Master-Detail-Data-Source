Imports System
Imports System.Windows.Forms
Imports System.IO
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.Sql
Imports DevExpress.DataAccess.Excel
Imports DevExpress.DataAccess.DataFederation
Imports System.ComponentModel
Imports System.Drawing
Imports DevExpress.XtraReports.UI

Namespace BindReportToFederatedMasterDetailDataSource
	Partial Public Class Form1
		Inherits Form

		Public Sub New()
			InitializeComponent()
		End Sub
		Private Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button1.Click
			Dim designTool As New ReportDesignTool(CreateReport())
			designTool.ShowRibbonDesignerDialog()
		End Sub
		Private Shared Function CreateFederationDataSource(ByVal sql As SqlDataSource, ByVal excel As ExcelDataSource) As FederationDataSource
			' Create SQL and Excel sources.
			Dim sqlSource As New Source(sql.Name, sql, "Categories")
			Dim excelSource As New Source(excel.Name, excel, "")

			' Create the federated "Categores" query and select data from the SQL source's query.
			Dim categoriesNode = sqlSource.From().Select("CategoryID", "CategoryName", "Description").Build("Categories")
			' Create the federated "Products" query and select data from the Excel data source.
			Dim productsNode = excelSource.From().Select("ProductName", "CategoryID", "UnitPrice").Build("Products")

			' Create a federated data source and add the federated queries to the collection.
			Dim federationDataSource = New FederationDataSource()
			federationDataSource.Queries.AddRange( { categoriesNode, productsNode })
			' Specify a master-detail relationship between these queries based on the "CategoryID" key field.
			Dim relation = New FederationMasterDetailInfo("Categories", "Products", New FederationRelationColumnInfo("CategoryID", "CategoryID"))
			federationDataSource.Relations.Add(relation)
			' Build the data source schema to display it in the Field List.
			federationDataSource.RebuildResultSchema()

			Return federationDataSource
		End Function
		Public Shared Function CreateReport() As XtraReport
			' Create a new report.
			Dim report = New XtraReport()

			' Add the Detail band to the master report and create a label bound to the "CategoryName" data field.
			Dim detailBand = New DetailBand() With {.HeightF = 25}
			Dim categoryLabel = New XRLabel() With {.WidthF = 150}
			categoryLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[CategoryName]"))
			detailBand.Controls.Add(categoryLabel)

			' Create the detail report and add the Detail band to it.
			Dim detailReport = New DetailReportBand()
			Dim detailBand2 = New DetailBand() With {.HeightF = 25}
			detailReport.Bands.Add(detailBand2)
			report.Bands.AddRange(New Band() { detailBand, detailReport })
			' Add a label bound to the "ProductName" data field.
			Dim productLabel = New XRLabel() With {
				.WidthF = 300,
				.LocationF = New PointF(100, 0)
			}
			productLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[ProductName]"))
			detailBand2.Controls.Add(productLabel)

			' Create data sources. 
			Dim sqlDataSource = CreateSqlDataSource()
			Dim excelDataSource = CreateExcelDataSource()
			Dim federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource)
			' Add all data sources to the report to avoid serialization issues. 
			report.ComponentStorage.AddRange(New IComponent() { sqlDataSource, excelDataSource, federationDataSource })
			' Assign a federated data source to the report and its detail report.
			report.DataSource = federationDataSource
			report.DataMember = "Categories"
			detailReport.DataSource = federationDataSource
			detailReport.DataMember = "Categories.CategoriesProducts"

			Return report
		End Function
		Private Shared Function CreateSqlDataSource() As SqlDataSource
			Dim connectionParameters = New Access97ConnectionParameters(Path.Combine(Path.GetDirectoryName(GetType(Form1).Assembly.Location), "Data/nwind.mdb"), "", "")
			Dim sqlDataSource = New SqlDataSource(connectionParameters) With {.Name = "Sql_Categories"}
			Dim categoriesQuery = SelectQueryFluentBuilder.AddTable("Categories").SelectAllColumnsFromTable().Build("Categories")
			sqlDataSource.Queries.Add(categoriesQuery)
			sqlDataSource.RebuildResultSchema()
			Return sqlDataSource
		End Function
		Private Shared Function CreateExcelDataSource() As ExcelDataSource
			Dim excelDataSource = New ExcelDataSource() With {.Name = "Excel_Products"}
			excelDataSource.FileName = Path.Combine(Path.GetDirectoryName(GetType(Form1).Assembly.Location), "Data/Products.xlsx")
			excelDataSource.SourceOptions = New ExcelSourceOptions() With {.ImportSettings = New ExcelWorksheetSettings("Sheet")}
			excelDataSource.RebuildResultSchema()
			Return excelDataSource
		End Function
	End Class
End Namespace
