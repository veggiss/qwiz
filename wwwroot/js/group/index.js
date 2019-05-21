$(document).ready(() => {
    let getSearchText = s => s.trim().length === 0 ? undefined : s;
    
    new Vue({
        el: '#tabContent',
        data: {
            groups: {
                currentPage: 1,
                entries: [],
                pages: 0
            }
        },
        mounted: function() {
            $(".onChangeCallback").change(() => this.getGroupsList(1));
            $("#searchInput").keypress(this.searchCallback);
            
            this.getGroupsList(1);
        },
        methods: {
            getGroupsList: function(page) {
                let self = this;

                axios.get(util.apiUrl('/api/group/getList/all', {
                    page: page,
                    size: global.membersCardAmount,
                    orderBy: $("#orderBySelect option:selected").attr("value"),
                    search: getSearchText($("#searchInput").val())
                })).then(function(response) {
                    self.groups.entries = response.data.entries;
                    self.groups.pages = response.data.pages;
                    self.groups.currentPage = page;
                });
            },
            searchCallback: function(e) {
                if (e.keyCode === 13) {
                    this.getGroupsList(1);
                }
            }
        }
    });
});