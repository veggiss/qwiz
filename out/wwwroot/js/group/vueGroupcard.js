Vue.component('groupcard', {
    props: ['group'],
    template:
 `
<a class="card float-left btn w-100 m-1" style="max-width: 16em;" :href="'/group/' + group.id">
    <div class="w-100 p-2 text-center">
        <p class="m-0 w-100 small text-muted">{{ group.creationDate }}</p>
        <p class="m-0 w-100 lead">{{ group.name }}</p>
        <p class="m-0 w-100 small lead">Owner: {{ group.ownerUsername }}</p>
        <p class="m-0 w-100 small lead">Members: {{ group.members }}</p>
        <p class="m-0 w-100 small lead">Region: {{ group.region }}</p>
        <p class="m-0 w-100 small lead bg-success" v-if="group.isPublic">Public Group</p>
        <p class="m-0 w-100 small lead bg-danger" v-else>Private Group</p>
    </div>
</a>
`
});