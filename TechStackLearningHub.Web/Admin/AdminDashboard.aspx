<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.AdminDashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Admin Dashboard</h1>
    <p class="lead">Overview of the learning hub.</p>

    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="card text-bg-primary">
                <div class="card-body">
                    <h2 class="card-title mb-0"><asp:Literal ID="litPublishedCourses" runat="server" /></h2>
                    <span>Published courses</span>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-bg-success">
                <div class="card-body">
                    <h2 class="card-title mb-0"><asp:Literal ID="litActiveStudents" runat="server" /></h2>
                    <span>Active students</span>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-bg-warning" style="--bs-bg-opacity: 1;">
                <div class="card-body">
                    <h2 class="card-title mb-0"><asp:Literal ID="litAttemptsWeek" runat="server" /></h2>
                    <span>Quiz attempts in last 7 days</span>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-bg-info">
                <div class="card-body">
                    <h2 class="card-title mb-0"><asp:Literal ID="litTotalUsers" runat="server" /></h2>
                    <span>Registered users</span>
                </div>
            </div>
        </div>
    </div>

    <h3 class="mt-4">Recent quiz attempts</h3>
    <asp:GridView ID="grdRecent" runat="server" AutoGenerateColumns="false" CssClass="table table-striped" EmptyDataText="No attempts yet.">
        <Columns>
            <asp:BoundField DataField="Username" HeaderText="Student" />
            <asp:BoundField DataField="QuizTitle" HeaderText="Quiz" />
            <asp:BoundField DataField="ModuleTitle" HeaderText="Module" />
            <asp:BoundField DataField="CourseName" HeaderText="Course" />
            <asp:TemplateField HeaderText="Score">
                <ItemTemplate><%# Eval("Score") %>%</ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Result">
                <ItemTemplate>
                    <asp:Label runat="server" Text="Passed" CssClass="badge text-bg-success" Visible='<%# (bool)Eval("IsPassed") %>' />
                    <asp:Label runat="server" Text="Not passed" CssClass="badge text-bg-danger" Visible='<%# !(bool)Eval("IsPassed") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="AttemptDate" HeaderText="Date" DataFormatString="{0:g}" />
        </Columns>
    </asp:GridView>
</asp:Content>