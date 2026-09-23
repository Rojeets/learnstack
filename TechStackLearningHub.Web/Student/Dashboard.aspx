<%@ Page Title="My Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="TechStackLearningHub.Web.Student.Dashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>My Dashboard</h1>
    <p class="lead">Welcome back. Here is your progress across the courses you have started.</p>
    <asp:Repeater ID="rptCourses" runat="server">
        <ItemTemplate>
            <div class="card mb-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <h5 class="card-title"><%# Eval("CourseName") %></h5>
                        <span class="badge bg-secondary"><%# Eval("TechStack") %></span>
                    </div>
                    <p class="card-text"><%# Eval("PercentComplete") %>% complete</p>
                    <div class="progress" role="progressbar" aria-valuenow="<%# Eval("PercentComplete") %>" aria-valuemin="0" aria-valuemax="100">
                        <div class="progress-bar" style='width: <%# Eval("PercentComplete") %>%'></div>
                    </div>
                    <a class="btn btn-sm btn-outline-primary mt-2" href='CourseDetails.aspx?CourseID=<%# Eval("CourseID") %>'>Continue</a>
                </div>
            </div>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div class="alert alert-info">You have not started any courses yet. <a href="CourseCatalog.aspx">Browse the catalogue</a>.</div>
        </EmptyDataTemplate>
    </asp:Repeater>
</asp:Content>