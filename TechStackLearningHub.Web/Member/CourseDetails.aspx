<%@ Page Title="Course Details" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="CourseDetails.aspx.cs" Inherits="TechStackLearningHub.Web.Member.CourseDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <a class="btn btn-link ps-0" href="CourseCatalog.aspx">&laquo; Back to catalogue</a>

    <asp:Panel ID="pnlCourse" runat="server">
        <div class="d-flex justify-content-between align-items-center">
            <h1><asp:Literal ID="litCourseName" runat="server" /></h1>
            <span class="badge bg-primary"><asp:Literal ID="litTechStack" runat="server" /></span>
        </div>
        <p class="lead"><asp:Literal ID="litDescription" runat="server" /></p>

        <p class="fw-semibold">Overall progress</p>
        <div class="progress" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100">
            <div class="progress-bar" id="barProgress" runat="server"></div>
        </div>

        <asp:Repeater ID="rptModules" runat="server">
            <ItemTemplate>
                <div class="card mt-4">
                    <div class="card-header">
                        <strong><%# Eval("ModuleTitle") %></strong>
                        <span class="text-muted float-end">Module <%# Container.ItemIndex + 1 %></span>
                    </div>
                    <div class="card-body">
                        <ul class="list-group list-group-flush">
                            <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand">
                                <ItemTemplate>
                                    <li class="list-group-item d-flex justify-content-between align-items-center">
                                        <span><%# Eval("LessonTitle") %></span>
                                        <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-primary"
                                            CommandName="Open" CommandArgument='<%# Eval("LessonID") %>' Text="Open lesson" />
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                        <asp:Repeater ID="rptQuizzes" runat="server">
                            <ItemTemplate>
                                <div class="d-flex justify-content-between align-items-center mt-3">
                                    <span class="fw-semibold"><%# Eval("QuizTitle") %></span>
                                    <a class="btn btn-sm btn-success" href='QuizPage.aspx?ModuleID=<%# Eval("ModuleID") %>'>Take quiz</a>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="alert alert-warning">This course is not available.</div>
    </asp:Panel>
</asp:Content>