import CompanyForm from "../CompanyForm";
import _mock from '../../../../assets/mock.png'

const CompanyCreate = () => {
    return <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar">
        <div className="text-center">
            <h1 className="font-bold text-neutral-900 text-5xl my-4">Cadastro da Companhia</h1>
            <p className="text-neutral-400 text-2xl font-medium">Aqui você pode alterar as regras de negócio da companhia <br /> e essas alterações são geram impacto no cálculo das propostas.</p>
        </div>
        <CompanyForm/>
    </div>
}

export default CompanyCreate;