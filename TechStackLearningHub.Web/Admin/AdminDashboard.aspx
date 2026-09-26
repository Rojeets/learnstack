<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.AdminDashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="card stat-card border-start border-4 border-primary">
                <div class="card-body">
                    <div class="text-muted small">Active students</div>
                    <div class="stat-value"><asp:Literal ID="litActiveStudents" runat="server" /></div>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card stat-card border-start border-4 border-success">
                <div class="card-body">
                    <div class="text-muted small">Published courses</div>
                    <div class="stat-value"><asp:Literal ID="litPublishedCourses" runat="server" /></div>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card stat-card border-start border-4 border-warning">
                <div class="card-body">
                    <div class="text-muted small">Quiz attempts in last 7 days</div>
                    <div class="stat-value"><asp:Literal ID="litAttemptsWeek" runat="server" /></div>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card stat-card border-start border-4 border-info">
                <div class="card-body">
                    <div class="text-muted small">Avg. pass rate</div>
                    <div class="stat-value"><asp:Literal ID="litAvgPassRate" runat="server" /></div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3">
        <div class="col-lg-7">
            <div class="card">
                <div class="card-header">Recent quiz attempts</div>
                <div class="card-body p-0">
                    <asp:GridView ID="grdRecent" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0" EmptyDataText="No attempts yet.">
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
                </div>
            </div>
        </div>
        <div class="col-lg-5">
            <div class="card">
                <div class="card-header">Courses by tech stack</div>
                <div class="card-body">
                    <asp:GridView ID="grdStacks" runat="server" AutoGenerateColumns="false" CssClass="stack-list w-100" GridLines="None" ShowHeader="false" EmptyDataText="No courses yet.">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <div class="stack-bar-row">
                                        <div class="stack-bar-label">
                                            <span><%# Eval("TechStack") %></span>
                                            <span><%# Eval("CourseCount") %></span>
                                        </div>
                                        <div class="progress" role="progressbar"
                                            aria-label='<%# GetStackBarLabel(Container.DataItem) %>'
                                            aria-valuenow='<%# GetPercentNumber(Eval("PercentOfMax")) %>'
                                            aria-valuemin="0" aria-valuemax="100">
                                            <div class="progress-bar bg-primary" style='width: <%# GetPercent(Eval("PercentOfMax")) %>'></div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
