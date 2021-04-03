<template>
<div class='app-container'>
    <el-dialog :title='title' :visible.sync='dialogVisible' :append-to-body='true'>
        <el-form ref="authform" :model="authform" label-width="120px" label-position='right' :rules="authformrules">
            <el-form-item label='通道类型'>
                <el-select :disabled='isModify' value-key="key" v-model='selectTemplate' placeholder='请选择' @change="selectTemplateChange">
                    <el-option v-for="item in sendTemplates" :key="item.key" :label="item.name" :value="item">
                    </el-option>
                </el-select>
            </el-form-item>
            <el-form-item label="名称">
                <el-input v-model="selectTemplate.name"></el-input>
            </el-form-item>
            <el-form-item v-for="(item) in selectTemplate.values" :label="item.description" :key="item.name" required>
                <el-input v-model="item.value" :placeholder="item.default" :readonly="item.readonly"></el-input>
            </el-form-item>
        </el-form>

        <template #footer>
            <span class='dialog-footer'>
                <el-button @click='dialogVisible = false'>取 消</el-button>
                <el-button type='primary' @click="submitForm('authform')">确 定</el-button>
            </span>
        </template>
    </el-dialog>
    <el-button type='primary' icon='el-icon-edit' @click='addAuth'>新 增</el-button>
    <el-table v-loading='listLoading' :data='sendAuthinfos' element-loading-text='加载中' border fit highlight-current-row>
        <el-table-column align='center' label='序号' width='95'>
            <template slot-scope='scope'>
                {{ scope.$index+1 }}
            </template>
        </el-table-column>
        <el-table-column label='类型' width='110' align='center'>
            <template slot-scope='scope'>
                <span>{{ scope.row.type }}</span>
            </template>
        </el-table-column>
        <el-table-column label='名称' width='110' align='center'>
            <template slot-scope='scope'>
                <span>{{ scope.row.name }}</span>
            </template>
        </el-table-column>
        <el-table-column label='配置信息' align='center'>>
            <template slot-scope='scope'>
                {{ scope.row.authData }}
            </template>
        </el-table-column>
        <el-table-column align='center' prop='created_at' label='编辑' width='200'>
            <template slot-scope='scope'>
                <el-button-group>
                    <el-button type='primary' icon='el-icon-edit' @click='modifyAuth(scope.$index, scope.row)'></el-button>
                    <el-button type='danger' icon='el-icon-delete' @click='deleteAuth(scope.$index, scope.row)'></el-button>
                </el-button-group>
            </template>
        </el-table-column>
        <el-table-column class-name='status-col' label='激活' width='110' align='center'>
            <template slot-scope='scope'>
                <el-switch active-color='#13ce66' v-model='scope.row.isActive' @change='activeAuth(scope.$index,scope.row,scope.row.isActive)' inactive-color='#ff4949'></el-switch>
            </template>
        </el-table-column>
    </el-table>
</div>
</template>

<script>
import {
    getSendAuths,
    deleteAuthInfo,
    activeAuthInfo,
    modifySendAuth,
    getSendTemplates,
    addAuthInfo,
    deepClone,
    getSendKey

} from '@/api/setting'

export default {
    data() {
        return {
            authformrules: {
                name: [{
                    required: true,
                    message: '必填'
                }],
                value: [{
                    required: true,
                    message: '必填'
                }]

            },
            sendAuthinfos: [],
            sendTemplates: [],
            selectTemplate: {
                key: 0,
                name: '',
                Values: []
            },
            selectTemplateKey: '',
            formLabelWidth: '120px',
            listLoading: true,
            dialogVisible: false,
            isModify: false,
            authform: {},
            title: "设置",
            sendKey: ""
        }
    },
    created() {
        this.fetchData()
    },
    methods: {
        fetchData() {
            this.listLoading = true
            getSendAuths().then((response) => {
                this.sendAuthinfos = response.data
                this.listLoading = false
            })
            getSendTemplates().then((response) => {
                if (response.code == 200) {
                    this.sendTemplates = response.data;
                }
            })
            getSendKey().then(response => {
                if (response.code == 200) {
                    this.sendKey = response.data;
                }
            })
        },
        selectTemplateChange(selectTemplate) {
            this.selectTemplate = deepClone(selectTemplate)
            if (this.selectTemplate.warning) {
                this.$message({
                    message: this.selectTemplate.warning,
                    type: 'warning'
                })
            }
        },
        submitForm(formName) {
            this.$refs[formName].validate((valid) => {
                if (valid) {
                    let allReady = this.selectTemplate.values.every(u => u.value);
                    if (allReady) {
                        if (this.isModify) {
                            modifySendAuth(this.selectTemplate).then((response) => {
                                if (response.code == 200) {
                                    this.$message({
                                        message: '修改成功',
                                        type: 'success'
                                    })
                                    this.dialogVisible = false;
                                    this.fetchData()
                                } else {
                                    this.$message.error('修改失败');
                                }
                            })
                        } else {
                            addAuthInfo(this.selectTemplate).then((response) => {
                                if (response.code == 200) {
                                    this.$message({
                                        message: '新增成功',
                                        type: 'success'
                                    })
                                    this.dialogVisible = false;
                                    this.fetchData()
                                } else {
                                    this.$message.error('新增失败');
                                }
                            })
                        }

                    } else {
                        this.$message.error('请填写完整!');
                    }

                    return false;
                } else {

                    return false;
                }
            });
        },
        addAuth() {
            this.title = '新增设置'
            this.dialogVisible = true
            this.isModify = false
            this.selectTemplate = deepClone(this.sendTemplates[0])
        },

        modifyAuth(index, row) {
            this.title = '修改设置'
            this.isModify = true;
            this.selectTemplate = deepClone(row);
            if (this.selectTemplate.type == "Bark") {
                let wPath = window.document.location.href;
                let pathName = this.$route.path;
                let pos = wPath.indexOf(pathName);
                let localhostPath = wPath.substring(0, pos);
                localhostPath = localhostPath.replace("#", "");

                var devieItem = this.selectTemplate.values.find(item => {
                    return item.name == "DeviceKey"
                })
                var sendUrlItem = this.selectTemplate.values.find(item => {
                    return item.name == "SendUrl"
                });
                sendUrlItem.value = localhostPath + "?act=" + this.sendKey + "/" + devieItem.value + "/{title}/{data}"
            }

            this.dialogVisible = true;
        },
        deleteAuth(index, row) {
            deleteAuthInfo(row.sendAuthId).then((response) => {
                if (response.code == 200) {
                    this.$message({
                        message: '删除成功',
                        type: 'success'
                    })
                    this.fetchData()
                } else {
                    this.$message.error('删除失败');
                }
            })
        },
        activeAuth(index, row, state) {
            activeAuthInfo(row.sendAuthId, state).then((response) => {
                if (response.code == 200) {
                    this.$message({
                        message: state ? '激活成功' : '注销成功',
                        type: 'success'
                    })
                    this.fetchData()
                } else {
                    this.$message.error(state ? '激活失败' : '注销失败');
                }
            })
        }
    }
}
</script>

<style lang="scss">
@media screen and (min-width:800px) {
    .el-dialog__wrapper .el-dialog {
        width: 800px !important;
    }

    .el-dialog__wrapper .el-dialog .el-dialog__body {
        overflow: auto
    }
}

@media screen and (max-width: 800px) {
    .el-dialog__wrapper .el-dialog {
        width: 99% !important;
    }

    .el-dialog__wrapper .el-dialog .el-dialog__body {
        overflow: auto
    }
}
</style>
