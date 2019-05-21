Vue.component('membercard', {
    props: ['member', 'ownerUsername', 'role'],
    template:
`
<a class="card float-left w-100 m-1" style="max-width: 16em;">
    <div class="w-100">
        <div v-if="role == 0 && member.role > 0">
            <button type="button" class="close remove ml-1 position-absolute" v-on:click="$emit('request', 'remove', member.username)">
                <span aria-hidden="true" title="Remove User" style="color:#dc3545;">&times;</span>
            </button>
        </div>
        <div v-if="role == 1 && member.role == 2" v-on:click="$emit('request', 'remove', member.username)">
            <button type="button" class="close remove ml-1 position-absolute">
                <span aria-hidden="true" title="Remove User" style="color:#dc3545;">&times;</span>
            </button>
        </div>
        <div class="float-left">
            <img style="width: 90px; height: auto;" class="m-1 avatar" src="../images/avatar.png">
        </div>
        <div class="float-left text-center m-1" style="width: 147px">
            <p class="m-0 w-100 small text-muted">{{ member.joinDate }}</p>
            <a class="m-0 w-100 small badge badge-primary" :href="'/Profile?username' + member.username">{{ member.username }}</a>
            <p class="m-0 w-100 small badge badge-info">Level {{ member.level }}</p>
            
            <div v-if="role == 0 && member.role > 0">
                <select v-on:change="$emit('request', 'change', member.username, $event)">
                    <option value="1" :selected="member.role == 1">Admin</option>
                    <option value="2" :selected="member.role == 2">Member</option>
                </select>
            </div>
            <div v-else>
                <p v-if="member.role == 0" class="m-0 w-100 small badge badge-danger">{{ member.roleText }}</p>
                <p v-else-if="member.role == 1" class="m-0 w-100 small badge badge-warning">{{ member.roleText }}</p>
                <p v-else class="m-0 w-100 small badge badge-success">{{ member.roleText }}</p>
            </div>
        </div>
    </div>
</a>
`
});