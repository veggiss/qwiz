Vue.component('pendingcard', {
    props: ['user'],
    template:
        `
<a class="card float-left w-100 m-1" style="max-width: 16em;">
    <div class="w-100">
        <div class="float-left">
            <img style="width: 90px; height: auto;" class="m-1 avatar" src="../images/avatar.png">
        </div>
        <div class="float-left text-center m-1" style="width: 147px">
            <a class="m-0 w-100 small badge badge-primary" :href="'/Profile?username' + user.username">{{ user.username }}</a>
            <p class="m-0 w-100 small badge badge-info">Level {{ user.level }}</p>
            
            <div class="w-100 btn-group btn-group-sm mt-2" role="group" aria-label="...">
                <button class="btn btn-success" v-on:click="$emit('answer', 'accept', user.username)">Accept</button>
                <button class="btn btn-danger"  v-on:click="$emit('answer', 'deny', user.username)">Deny</button>
            </div>
        </div>
    </div>
</a>
`
});