Vue.component('leaderboardCard', {
props: ['users', 'page', 'score'],
template:
`
<div class="w-100">
    <table class="table">
        <thead>
        <tr>
            <th scope="col">#</th>
            <th scope="col">Username</th>
            <th class="text-center" scope="col">Score</th>
            <th class="text-center" scope="col">Quizzes Taken</th>
        </tr>
        </thead>
        <tbody>
            <tr v-for="(user, index) in users">
                <th>{{ (index + 1) + ((page - 1) * 10) }}</th>
                <td><img class="leaderboardavatar mr-2" :src="user.imagePath"/>[Level {{ user.level }}] - <a :href="'/user/' + user.userName">{{ user.userName }}</a></td>
                <td class="text-center" v-if="score">{{ user.score }}</td>
                <td class="text-center">{{ user.quizzesTakenCount }}</td>
            </tr>
        </tbody>
    </table>
</div>

`
});

Vue.component('quizLeaderboardCard', {
    props: ['users', 'page'],
    template:
`
<div class="w-100">
    <table class="table">
        <thead>
        <tr>
            <th scope="col">#</th>
            <th scope="col">Username</th>
            <th class="text-center" scope="col">Date Taken</th>
            <th class="text-center" scope="col">Score</th>
            <th class="text-center" scope="col">Correct Answers</th>
        </tr>
        </thead>
        <tbody>
            <tr v-for="(user, index) in users">
                <th>{{ (index + 1) + ((page - 1) * 10) }}</th>
                <td><img class="leaderboardavatar mr-2" :src="user.user.imagePath"/>[{{ user.user.level }}] - <a :href="'/user/' + user.takerUsername">{{ user.takerUsername }}</a></td>
                <th class="text-center">{{ user.dateTakenFormatted }}</th>
                <td class="text-center">{{ user.score }}</td>
                <td class="text-center">{{ user.correctAnswers }} / {{ user.questionsLength }}</td>
            </tr>
        </tbody>
    </table>
</div>

`
});