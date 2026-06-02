using BaiTapWeb_Buoi5.Models;

namespace BaiTapWeb_Buoi5.Repositories;

public interface ISliderRepository
{
    Task<IEnumerable<SliderImage>> GetAllAsync();
    Task<SliderImage?> GetByIdAsync(int id);
    Task AddAsync(SliderImage sliderImage);
    Task DeleteAsync(int id);
    Task UpdateDisplayOrdersAsync(IDictionary<int, int> displayOrders);
}
