<template>
<div class="app-container">
    <el-card>
        <template #header>
            <div class="card-header">
                <span>消息验证</span>
            </div>
        </template>
        <el-form ref="sendform" :model="messageForm" label-width="20%" :rules="messageForm.rules">
            <el-form-item label="消息标题" prop='title'>
                <el-input type="text" v-model="messageForm.title" placeholder="必填"> </el-input>
            </el-form-item>
            <el-form-item label="消息内容" prop='data'>
                <el-input type="textarea" v-model="messageForm.data" placeholder="选填"></el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="onMessage('sendform')">发送</el-button>
            </el-form-item>
        </el-form>

        <el-divider content-position="left">重置授权</el-divider>
        <el-form ref="resetform" :model="keyForm" label-width="20%">
            <el-form-item label="当前SendKey">
                <el-input v-model="keyForm.sendKey" :readonly="true"></el-input>
            </el-form-item>
            <el-form-item label="快捷地址(标题)">
                <el-input type="textarea" v-model="keyForm.sendUrlTitle" :readonly="true"></el-input>
            </el-form-item>
             <el-form-item label="快捷地址(完整)">
                <el-input type="textarea" v-model="keyForm.sendUrl" :readonly="true"></el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="onReSendKey('resetform')">重置SendKey</el-button>
            </el-form-item>
        </el-form>
    </el-card>
</div>
</template>

<script>
import {
    getSendKey,
    reSendKey,
    sendMessage
} from "@/api/setting";

export default {
    filters: {
        statusFilter(status) {
            const statusMap = {
                published: "success",
                draft: "gray",
                deleted: "danger",
            };
            return statusMap[status];
        },
    },
    data() {
        return {
            listLoading: true,
            messageForm: {
                title: '',
                data: '',
                sendKey: '',
                rules: {

                    title: [{
                        required: true,
                        message: '请输入标题',
                        trigger: 'blur'
                    }],
                    data: [{
                        required: false,
                    }]
                }
            },
            keyForm: {
                sendKey: '',
                sendUrl: '',
            },
            sendTemplates: []
        };
    },
    created() {
        this.fetchData();
    },
    methods: {
        fetchData() {
            this.listLoading = true;
            getSendKey().then((response) => {
                this.keyForm.sendKey = response.data;
                this.messageForm.sendKey = response.data;
                let wPath = window.document.location.href;
                let pathName = this.$route.path;
                let pos = wPath.indexOf(pathName);
                let localhostPath = wPath.substring(0, pos);
                localhostPath = localhostPath.replace("#", "");
                this.keyForm.sendUrlTitle = localhostPath + 'api/'+ this.keyForm.sendKey+'.send' + "/{title}"
                this.keyForm.sendUrl = localhostPath + 'api/'+ this.keyForm.sendKey+'.send' + "/{title}/{data}"
                // this.keyForm.sendUrl = localhostPath + 'api/send?token=' + this.keyForm.sendKey + "&title={title}&data={data}"
                this.listLoading = false;
            });
        },
        onMessage(fromname) {
            this.$refs[fromname].validate(valid => {

                if (valid) {
                    let url = this.keyForm.sendUrl.replace("{{title}}}", this.messageForm.title);
                    url.replace("{{data}}}", this.messageForm.data);
                    sendMessage({
                        token: this.keyForm.sendKey,
                        title: this.messageForm.title,
                        data: this.messageForm.data
                    }).then((response) => {
                        if (response.code == 200) {
                            this.$message({
                                message: '发送成功',
                                type: 'success'
                            });
                        } else {
                            this.$message.error('发送失败');
                        }
                    });
                }

            });
        },
        onReSendKey() {
            reSendKey().then((response) => {
                if (response.code == 200) {
                    this.$message({
                        message: '重置成功',
                        type: 'success'
                    });
                    this.fetchData();
                } else {
                    this.$message.error('重置失败');
                }

            });
        }
    },
};
</script>

<style lang="scss">
.sendkey {
    width: 300px;
    display: inline-block;
}

.box-card {
    margin: 5px;
}
</style>
