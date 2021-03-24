<template>
<div class="app-container">
    <el-card>
        <template #header>
            <div class="card-header">
                <span>参数设置</span>
            </div>
        </template>
        <el-form ref="settingForm" :model="settingForm" label-width="20%" label-position="right">
            <el-form-item prop="sendthread" label="发送线程">
                <el-input ref="sendthread" name="thread" v-model="settingForm.sendthread" placeholder="2">></el-input>
            </el-form-item>
            <el-divider content-position="left">管理权限</el-divider>
            <el-form-item label="用户名">
                <el-input v-model="settingForm.administrators" placeholder="admin|demon">></el-input>
            </el-form-item>
            <el-divider content-position="left">代理设置</el-divider>
            <el-form-item prop="proxy" label="代理地址">
                <el-input ref="proxy" name="proxy" v-model="settingForm.proxy" placeholder="http://127.0.0.1:1080"></el-input>
            </el-form-item>
            <el-form-item prop="proxy" label="启用代理">
                <template>
                    <el-switch active-color='#13ce66' v-model='settingForm.proxyenable' inactive-color='#ff4949'></el-switch>
                </template>
            </el-form-item>

            <el-divider content-position="left">Github登陆</el-divider>
            <el-form-item label="应用ID">
                <el-input v-model="settingForm.githubClientID" placeholder="Client ID">></el-input>
            </el-form-item>
            <el-form-item label="应用密钥">
                <el-input v-model="settingForm.githubClientSecret" placeholder="Client secrets"></el-input>
            </el-form-item>
            <el-form-item label="启用登陆">
                <template>
                    <el-switch active-color='#13ce66' v-model='settingForm.githubEnable' inactive-color='#ff4949'></el-switch>
                </template>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitForm('settingForm')">确认修改</el-button>
            </el-form-item>
        </el-form>
    </el-card>
</div>
</template>

<script>
import {
    getGlobal,
    setGlobal
} from '@/api/systemsetting'

export default {

    data() {
        return {
            settingForm: {
                sendthread: '',
                administrators: '',
                proxy: '',
                proxyenable: false,
                githubClientID: '',
                githubClientSecret: '',
                githubEnable: false
            },
            jwt: {

            }
        }
    },
    created() {
        this.fetchData()
    },
    methods: {
        fetchData() {

            getGlobal().then((resposne) => {
                this.settingForm = resposne.data
            });
        },
        submitForm(formName) {

            this.$confirm('您修改了全局参数，这将变更管理员及三方验证，确认修改吗', '警告', {
                distinguishCancelAndClose: true,
                confirmButtonText: '确定修改',
                cancelButtonText: '放弃修改',
                type: 'warning',
                center: true,
                callback: action => {
                    if (action == 'confirm') {
                        this.$refs[formName].validate((valid) => {
                            setGlobal(this.settingForm).then((response) => {
                                if (response.code == 200) {
                                    this.$message({
                                        message: '设置成功',
                                        type: 'success'
                                    });
                                } else {
                                    this.$message.error('设置失败');
                                }
                            })
                        })
                    }
                }
            })
        }
    }
}
</script>
