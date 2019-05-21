$(document).ready(() => {
    new Vue({
        el: '#tabContent',
        data: {
            groupAuthorized: model.isPublic || model.roleText != null,
            id: model.id,
            role: model.role,
            members: {
                entries: [],
                isPublic: null,
                currentPage: 1,
                pages: null,
                role: null
            },
            invites: {
                entries: [],
                currentPage: 1,
                pages: null
            }
        },
        mounted: function() {
            if (this.role == null) {
                $("#joinGroupBtn").click(() => this.requestGroup("join"));
            } else if (this.role === 0) {
                $("#deleteGroupBtn").click(this.deleteGroup);
            } else {
                $("#leaveGroupBtn").click(() => this.requestGroup("leave"));
            }
            
            if (this.groupAuthorized) {
                this.membersCallback(1);
                $(".onChangeCallback").change(() => this.membersCallback(1));
                
                if (this.role === 0 || this.role === 1) {
                    this.getPendingInvites(1);
                }
            }
        },
        methods: {
            membersCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/group/getList/members', {
                    id: self.id,
                    page: page,
                    size: global.membersCardAmount,
                    orderBy: $("#orderBySelect option:selected").attr("value")
                })).then(function(response) {
                    self.members.entries = response.data.entries;
                    self.members.pages = response.data.pages;
                    self.members.currentPage = page;
                });
            },
            getPendingInvites: function(page) {
                let self = this;

                axios.get(util.apiUrl('/api/group/getList/pending', {
                    id: self.id,
                    page: page,
                    size: global.membersCardAmount
                })).then(function(response) {
                    self.invites.entries = response.data.entries;
                    self.invites.pages = response.data.pages;
                }).catch(function(e) {
                    if (e.response.status === 400)
                        util.openModal(e.response.data);
                });
            },
            requestGroup: function(type, username, event) {
                if (global.isAuthenticated) {
                    let self = this;
                    
                    axios.put(util.apiUrl('/api/group/request', {
                        id: self.id,
                        type: type,
                        username: username,
                        role: event ? event.target.value : undefined
                    })).then(function(response) {
                        if (response.status === 200) {
                            if (type === "join") 
                                $("#requestBtnGroup").html("<button class='m-3 btn btn-success' disabled>Request Pending</button>");
                            else if (type === "remove")
                                self.membersCallback(self.members.currentPage);
                            else if (type === "accept" || type === "deny")
                                self.getPendingInvites(self.invites.currentPage);
                        }
                    }).catch(function(e) {
                        if (e.response.status === 400)
                            util.openModal(e.response.data);
                    });
                }
            },
            deleteGroup: function() {
                if (global.isAuthenticated && this.role === 0) {
                    let self = this;

                    axios.delete(util.apiUrl('/api/group/remove', {
                        id: self.id
                    })).then(function(response) {
                        console.log(response);
                    }).catch(function(e) {
                        if (e.response.status === 400)
                            util.openModal(e.response.data);
                    });
                }
            }
        }
    });
});